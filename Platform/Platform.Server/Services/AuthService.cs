using com.etsoo.Address;
using com.etsoo.ApiModel.Auth;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Database.Converters;
using com.etsoo.HTTP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Crypto;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.Serialization;
using com.etsoo.Utils.Storage;
using com.etsoo.Web;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Platform.Server.Application;
using Platform.Server.Dto.Auth;
using Platform.Server.Endpoints.Auth.RQ;
using Platform.Server.Endpoints.AuthCode.RQ;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Platform.Server.Services
{
    public class AuthService : CommonService, IAuthService
    {
        private record TokenQueryData
        {
            public required int DeviceId { get; init; }
            public required int UserId { get; init; }
            public required string Culture { get; init; }
            public required DeviceTokenData Data { get; init; }
        }

        private record AppData
        {
            public required string AppSecret { get; set; }
            public required string WebUrl { get; init; }
            public required string[] ApiUrls { get; init; }
        }

        private record MoreData
        {
            public required int DeviceId { get; init; }
            public required int? OrganizationId { get; init; }
        }

        private const string BearerTokenType = "Bearer";

        // 检查用户登录编号
        /// <summary>
        /// Check user login id
        /// 检查用户登录编号
        /// </summary>
        /// <param name="type">Identifier type</param>
        /// <param name="id">Email or mobile</param>
        /// <param name="region">Region</param>
        /// <returns>Action result</returns>
        static ActionResult CheckId(CoreUserIdentifierType type, ref string id, string region)
        {
            if (type == CoreUserIdentifierType.Email)
            {
                // Try parse
                if (MailAddress.TryCreate(id, out var emailAddress))
                {
                    id = emailAddress.Address;
                }
                else
                {
                    return ApplicationErrors.InvalidEmail.AsResult();
                }
            }
            else if (type == CoreUserIdentifierType.Mobile)
            {
                // Try parse and format
                var phone = AddressRegion.CreatePhone(id, region);
                if (phone != null)
                {
                    id = phone.ToInternationalFormat();
                }
                else
                {
                    return ApplicationErrors.InvalidMobile.AsResult();
                }
            }

            return ActionResult.Success;
        }

        readonly MyDbContext _db;
        readonly IStorage _storage;
        readonly IHttpClientFactory _httpClientFactory;
        readonly IPAddress _ip;
        readonly MinUserToken? _regUser;
        readonly IPublicService _publicService;
        readonly IAuthCodeService _authCodeService;
        readonly IQueueService _queueService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        /// <param name="httpClientFactory">HTTP client factory</param>
        /// <param name="publicService">Public service</param>
        /// <param name="authCodeService">Auth code service</param>
        /// <param name="queueService">Queue service</param>
        public AuthService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<AuthService> logger,
            IStorage storage, IHttpClientFactory httpClientFactory, IPublicService publicService, IAuthCodeService authCodeService,
            IQueueService queueService)
            : base(app, userAccessor.User, "auth", logger)
        {
            _db = db;
            _storage = storage;
            _httpClientFactory=httpClientFactory;

            _ip = userAccessor.Ip;
            _regUser = userAccessor.User == null ? userAccessor.CreateUserFromAuthorization<MinUserToken>(app.AuthService, MyAppConstants.RegistrationTokenAudience, MyAppConstants.RegistrationTokenScheme) : null;
            _publicService = publicService;
            _authCodeService = authCodeService;
            _queueService = queueService;
        }

        private async Task<AppData?> AuthGetAppSecretAsync(int appId, string appKey, CancellationToken cancellationToken)
        {
            AppData? data;

            if (string.IsNullOrEmpty(appKey))
            {
                data = await _db.CoreApps.AsNoTracking().Where(a => a.Id == appId).Select(a => new AppData { AppSecret = a.AppSecret, WebUrl = a.WebUrl, ApiUrls = a.ApiUrls }).FirstOrDefaultAsync(cancellationToken);
            }
            else
            {
                data = await _db.CoreOrganizationApps.AsNoTracking().Where(oa => oa.CoreAppId == appId && oa.AppKey == appKey && oa.AppSecret != null).Select(oa => new AppData { AppSecret = oa.AppSecret!, WebUrl = oa.LocalUrl ?? oa.CoreApp.WebUrl, ApiUrls = oa.LocalApis ?? oa.CoreApp.ApiUrls }).FirstOrDefaultAsync(cancellationToken);
            }

            if (data != null)
            {
                data.AppSecret = App.DecriptData(data.AppSecret, "Token");
            }

            return data;
        }

        private async Task<AppTokenData?> CreateAppTokenDataAsync(int appId, string refreshToken, string? appSecret, string timezone, CancellationToken cancellationToken)
        {
            var tokenData = await _db.CoreUserDeviceTokens
                .AsNoTracking()
                .Where(d => d.ResponseType == TokenResponseType.Token && d.AppId == appId && d.Token == refreshToken)
                .Select(d => new
                {
                    d.Id,
                    d.Expiry,
                    Data = new TokenQueryData
                    {
                        DeviceId = d.DeviceId,
                        UserId = d.Device.CoreUserId,
                        Culture = d.Culture,
                        Data = d.Data
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (tokenData == null)
            {
                Logger.LogWarning("Token query data not found");
                return null;
            }
            else if (tokenData.Expiry < DateTime.UtcNow)
            {
                // Remove the token
                await _db.CoreUserDeviceTokens.Where(t => t.Id == tokenData.Id).ExecuteDeleteAsync(cancellationToken);

                Logger.LogWarning("Token expired: {Expiry}", tokenData.Expiry);
                return null;
            }

            var data = tokenData.Data;

            // User
            var (result, user) = await CreateUserFromQueryDataAsync(data, timezone, cancellationToken);
            if (user == null || !result.Ok)
            {
                Logger.LogWarning("User not found with @{data}, @{result}", data, result);
                return null;
            }

            var scope = CurrentUser.AppIdToScope(appId);
            if (user.Scopes?.Contains(scope) is not true)
            {
                Logger.LogWarning("User not in scope {scope}", scope);
                return null;
            }

            return await CreateAppTokenDataAsync(user, appId, appSecret, true, data, cancellationToken);
        }

        private async Task<AppTokenData> CreateAppTokenDataAsync(CurrentUser user, int appId, string? appSecret, bool isOffline, TokenQueryData data, CancellationToken cancellationToken)
        {
            var accessToken = App.AuthService.CreateAccessToken(user, null, App.AuthService.AccessTokenMinutes);

            string? refreshToken = null;
            if (isOffline)
            {
                refreshToken = await CreateRefreshTokenAsync(user.IdInt, data.DeviceId, data.Culture, TokenResponseType.Token, data.Data, appId, cancellationToken);
            }

            var idToken = string.IsNullOrEmpty(appSecret) ? null : App.AuthService.CreateIdToken(user.CreateIdentity(), appSecret);

            var token = new AppTokenData
            {
                AccessToken = accessToken,
                TokenType = BearerTokenType,
                ExpiresIn = App.AuthService.AccessTokenMinutes * 60,
                RefreshToken = refreshToken,
                Scope = string.Join(' ', user.Scopes!),
                IdToken = idToken
            };

            return token;
        }

        private async Task<string> CreateAuthCodeAsync(CurrentUser user, int appId, TokenQueryData data, CancellationToken cancellationToken)
        {
            var code = await CreateRefreshTokenAsync(user.IdInt, data.DeviceId, data.Culture, TokenResponseType.Code, data.Data, appId, cancellationToken);
            return code;
        }

        /// <summary>
        /// Refresh API token
        /// 刷新API令牌
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<ApiTokenData?> ApiRefreshTokenAsync(ApiRefreshTokenRQ rq, CancellationToken cancellationToken = default)
        {
            var data = await CreateAppTokenDataAsync(rq.AppId, rq.Token, null, rq.TimeZone, cancellationToken);
            if (data == null || string.IsNullOrEmpty(data.RefreshToken)) return null;

            return new ApiTokenData
            {
                AccessToken = data.AccessToken,
                TokenType = data.TokenType,
                ExpiresIn = data.ExpiresIn,
                RefreshToken = data.RefreshToken
            };
        }

        /// <summary>
        /// Authorization request
        /// 授权请求
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public ValueTask<string> AuthRequestAsync(AuthRequest rq, CancellationToken cancellationToken = default)
        {
            if (User == null)
            {
                throw new UnauthorizedAccessException();
            }

            return AuthRequestAsync(rq, User, null, cancellationToken);
        }

        /// <summary>
        /// Check user identifier exists
        /// 检查用户标识符是否存在
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<TristateEnum> CheckUserIdentifierAsync(CheckUserIdentifierRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            // Check device
            if (!this.CheckDevice(userAgent, rq.DeviceId, out var _, out var cd))
            {
                return TristateEnum.Unsure;
            }

            var deviceCore = cd.Value.DeviceCore;

            var openid = DecryptDeviceData(rq.Openid, deviceCore);
            if (string.IsNullOrEmpty(openid))
            {
                return TristateEnum.Unsure;
            }

            var data = new CheckUserIdentifierData
            {
                Type = rq.Type,
                Openid = openid,
                Region = rq.Region
            };

            return await CheckUserIdentifierAsync(data, cancellationToken);
        }

        /// <summary>
        /// Check user identifier exists
        /// 检查用户标识符是否存在
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<TristateEnum> CheckUserIdentifierAsync(CheckUserIdentifierData data, CancellationToken cancellationToken = default)
        {
            if (_regUser == null && User == null)
            {
                return TristateEnum.Unsure;
            }

            var openid = data.Openid;
            var result = CheckId(data.Type, ref openid, data.Region);
            if (!result.Ok)
            {
                return TristateEnum.Unsure;
            }

            var exists = await _db.CoreUserIdentifiers.AnyAsync(i => i.Type == data.Type && i.Value == openid, cancellationToken);
            return exists ? TristateEnum.True : TristateEnum.False;
        }

        private async Task<TokenQueryData?> CreateTokenQueryDataAsync(CurrentUser user, CancellationToken cancellationToken)
        {
            var data = await _db.CoreUserDeviceTokens.AsNoTracking().Where(t => t.DeviceId == user.DeviceIdInt && t.ResponseType == TokenResponseType.Token && t.AppId == null && t.Expiry >= DateTime.UtcNow)
                .OrderByDescending(t => t.Id)
                .Select(t => new TokenQueryData
                {
                    DeviceId = t.DeviceId,
                    UserId = user.IdInt,
                    Culture = t.Culture,
                    Data = t.Data
                })
                .FirstOrDefaultAsync(cancellationToken);

            return data;
        }

        /// <summary>
        /// Authorization request
        /// 授权请求
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="currentUser">Current user</param>
        /// <param name="tokenQueryData">Token query data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        private async ValueTask<string> AuthRequestAsync(AuthRequest rq, CurrentUser currentUser, TokenQueryData? tokenQueryData, CancellationToken cancellationToken = default)
        {
            // User for authorization, update the scopes
            var scopes = currentUser.Scopes?.Intersect(rq.Scopes);
            var user = currentUser with { AppId = rq.AppId, Scopes = scopes };

            var result = ActionResult.Success;

            var redirectUri = rq.RedirectUri.ToString();
            var url = new StringBuilder(redirectUri);
            url.Append('?');

            var appData = await AuthGetAppSecretAsync(rq.AppId, rq.AppKey, cancellationToken);

            if (appData == null)
            {
                result = ApplicationErrors.NoValidData.AsResult("AppId");
            }
            else if (!redirectUri.StartsWith(appData.WebUrl) && !appData.ApiUrls.Any(redirectUri.StartsWith))
            {
                result = ApplicationErrors.NoValidData.AsResult("RedirectUri");
            }
            else
            {
                // Check the signature
                var expectedSignature = rq.SignWith(appData.AppSecret);

                if (!rq.Sign.Equals(expectedSignature))
                {
                    result = ApplicationErrors.NoValidData.AsResult("Sign");
                }
                else
                {
                    tokenQueryData ??= await CreateTokenQueryDataAsync(user, cancellationToken);

                    if (tokenQueryData == null)
                    {
                        result = ApplicationErrors.NoValidData.AsResult("TokenQueryData");
                    }
                    else
                    {
                        if (rq.ResponseType.Equals(AuthRequest.TokenResponseType))
                        {
                            var token = await CreateAppTokenDataAsync(user, rq.AppId, appData.AppSecret, rq.AccessType == AuthRequest.OfflineAccessType, tokenQueryData, cancellationToken);
                            var tokenJson = JsonSerializer.Serialize(token, ModelJsonSerializerContext.Default.AppTokenData);
                            url.Append($"token={HttpUtility.UrlEncode(tokenJson)}");
                        }
                        else if (rq.ResponseType.Equals(AuthRequest.CodeResponseType))
                        {
                            var code = await CreateAuthCodeAsync(user, rq.AppId, tokenQueryData, cancellationToken);
                            url.Append($"code={HttpUtility.UrlEncode(code)}");
                        }
                        else
                        {
                            result = ApplicationErrors.NoValidData.AsResult("response_type");
                        }
                    }
                }
            }

            if (result.Ok)
            {
                url.Append($"&state={HttpUtility.UrlEncode(rq.State)}");
            }
            else
            {
                url.Append($"error={HttpUtility.UrlEncode(result.Title)}");
                if (!string.IsNullOrEmpty(result.Field))
                {
                    url.Append($"&error_field={HttpUtility.UrlEncode(result.Field)}");
                }
            }

            return url.ToString();
        }

        /// <summary>
        /// Complete registration
        /// 完成注册
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<(IActionResult result, string? refreshToken)> CompleteRegisterAsync(CompleteRegisterRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            if (_regUser == null)
            {
                return (ApplicationErrors.AccessDenied.AsResult(), null);
            }

            // Check device
            if (!this.CheckDevice(userAgent, rq.DeviceId, out var checkResult, out var cd))
            {
                return (checkResult, null);
            }

            var deviceCore = cd.Value.DeviceCore;

            var pasword = DecryptDeviceData(rq.Password, deviceCore);
            if (pasword == null)
            {
                return (ApplicationErrors.NoValidData.AsResult("Password"), null);
            }

            var deviceName = cd.Value.Parser.ToShortName();

            // Hash password
            var password = await App.HashPasswordAsync(_regUser.Id + pasword);

            // Update the user
            var user = await _db.CoreUsers.FirstOrDefaultAsync(u => u.Id == _regUser.IdInt, cancellationToken);
            if (user == null)
            {
                return (ApplicationErrors.NoValidData.AsResult("User"), null);
            }

            user.Password = password;
            user.Name = rq.Name;
            user.QueryKeyword = _publicService.GetPinyin(new PinyinRQ { Input = rq.Name, Format = PinyinFormatType.Initial });

            user.FamilyName = rq.FamilyName;
            if (!string.IsNullOrEmpty(rq.FamilyName))
            {
                user.LatinFamilyName = _publicService.GetPinyin(new PinyinRQ { Input = rq.FamilyName, Format = PinyinFormatType.Full });
            }

            user.GivenName = rq.GivenName;
            if (!string.IsNullOrEmpty(rq.GivenName))
            {
                user.LatinGivenName = _publicService.GetPinyin(new PinyinRQ { Input = rq.GivenName, Format = PinyinFormatType.Full });
            }

            user.Region = rq.Region;
            user.Step = 0;

            _db.CoreUsers.Update(user);
            await _db.SaveChangesAsync(cancellationToken);

            var (loginResult, refreshToken, _) = await CompleteLoginAsync(user, rq.DeviceId, deviceName, DeviceType.Web, rq.Region, null, null, rq.Auth, rq.Timezone, cancellationToken);

            return (loginResult, refreshToken);
        }

        /// <summary>
        /// Change user password
        /// 修改用户密码
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">HTTP user agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> ChangePasswordAsync(ChangePasswordRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            // Check device
            if (!this.CheckDevice(userAgent, rq.DeviceId, out var checkResult, out var cd))
            {
                return checkResult;
            }

            var deviceCore = cd.Value.DeviceCore;

            try
            {
                var oldPassword = DecryptDeviceData(rq.OldPassword, deviceCore);
                if (string.IsNullOrEmpty(oldPassword))
                {
                    return ApplicationErrors.NoValidData.AsResult("OldPassword");
                }

                var password = DecryptDeviceData(rq.Password, deviceCore);
                if (string.IsNullOrEmpty(password))
                {
                    return ApplicationErrors.NoValidData.AsResult("Password");
                }

                var dto = new ChangePasswordDto(oldPassword, password);

                return await ChangePasswordAsync(dto, cancellationToken);
            }
            catch (Exception ex)
            {
                return LogException(ex);
            }
        }

        /// <summary>
        /// Change password
        /// 修改密码
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> ChangePasswordAsync(ChangePasswordDto data, CancellationToken cancellationToken = default)
        {
            // Validate the user
            if (User == null)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var user = await _db.CoreUsers
                .AsNoTracking()
                .Where(u => u.Id == User.IdInt)
                .Select(u => new LoginUserWithPassword { Id = u.Id, Name = u.Name, Password = u.Password, Status = u.Status, FrozenTime = u.FrozenTime, Step = u.Step })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return ApplicationErrors.NoUserFound.AsResult();
            }

            var result = user.ValidateUser(User.TimeZone);
            if (!result.Ok)
            {
                return result;
            }

            // Hash password
            var hashedPassword = await App.HashPasswordAsync(user.Id + data.OldPassword);
            if (string.IsNullOrEmpty(hashedPassword) || !hashedPassword.Equals(user.Password))
            {
                return ApplicationErrors.NoPasswordMatch.AsResult();
            }

            // Update password
            var newPassword = await App.HashPasswordAsync(user.Id + data.Password);
            await _db.CoreUsers.AsNoTracking().Where(u => u.Id == user.Id).ExecuteUpdateAsync(u => u.SetProperty(u => u.Password, newPassword), cancellationToken);

            // Log
            await _queueService.PushAsync(new ChangePasswordMessage
            {
                Data = User.CreateMessageData(0)
            }, PlatformSharedContext.Default.ChangePasswordMessage, cancellationToken);

            return ActionResult.Success;
        }

        /// <summary>
        /// Login with password
        /// 使用密码登录
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<(IActionResult result, string? refreshToken)> LoginWithPwdAsync(LoginRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            // Check device
            if (!this.CheckDevice(userAgent, rq.DeviceId, out var checkResult, out var cd))
            {
                return (checkResult, null);
            }

            var deviceCore = cd.Value.DeviceCore;

            var id = DecryptDeviceData(rq.Id, deviceCore);
            if (string.IsNullOrEmpty(id) || id.Length < 6)
            {
                return (ApplicationErrors.NoValidData.AsResult(), null);
            }

            var password = DecryptDeviceData(rq.Pwd, deviceCore);
            if (string.IsNullOrEmpty(password))
            {
                return (ApplicationErrors.NoValidData.AsResult("Password"), null);
            }

            var (result, login) = await LoginIdAsync(id, rq.Region, rq.TimeZone, cancellationToken);
            if (!result.Ok || login == null)
            {
                return (result, null);
            }

            // Hash password
            var hashedPassword = await App.HashPasswordAsync(login.Id + password);
            if (string.IsNullOrEmpty(hashedPassword) || !hashedPassword.Equals(login.Password))
            {
                // Log
                await _queueService.PushAsync(new LoginFailedMessage
                {
                    Data = new CommonMessageData
                    {
                        Culture = CultureInfo.CurrentCulture.Name,
                        DeviceId = null,
                        IP = _ip.ToString(),
                        UserId = login.Id,
                        UserName = login.Name,
                        OrganizationId = null,
                        TimeZone = rq.TimeZone,
                        TargetId = 0
                    },
                    Reason = "Password",
                    UserAgent = userAgent
                }, PlatformSharedContext.Default.LoginFailedMessage, cancellationToken);

                return (ApplicationErrors.NoPasswordMatch.AsResult(), null);
            }

            var user = await _db.CoreUsers.AsNoTracking()
                .Where(u => u.Id == login.Id)
                .Select(u => new CoreUserLogin
                {
                    Id = u.Id,
                    Name = u.Name,
                    GivenName = u.GivenName,
                    FamilyName = u.FamilyName,
                    LatinGivenName = u.LatinGivenName,
                    LatinFamilyName = u.LatinFamilyName,
                    Avatar = u.Avatar,
                    LatestOrganizationIds = u.LatestOrganizationIds,
                    LatestAppIds = u.LatestAppIds
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return (ApplicationErrors.NoValidData.AsResult("User"), null);
            }

            var deviceName = cd.Value.Parser.ToShortName();

            var (loginResult, refreshToken, moreData) = await CompleteLoginAsync(user, rq.DeviceId, deviceName, DeviceType.Web, rq.Region, rq.Org, null, rq.Auth, rq.TimeZone, cancellationToken);

            if (loginResult.Ok)
            {
                // Log
                await _queueService.PushAsync(new LoginSuccessMessage
                {
                    Data = new CommonMessageData
                    {
                        Culture = CultureInfo.CurrentCulture.Name,
                        DeviceId = moreData?.DeviceId,
                        IP = _ip.ToString(),
                        UserId = login.Id,
                        UserName = login.Name,
                        OrganizationId = moreData?.OrganizationId,
                        TimeZone = rq.TimeZone,
                        TargetId = 0
                    },
                    UserAgent = userAgent
                }, PlatformSharedContext.Default.LoginSuccessMessage, cancellationToken);
            }
            else
            {
                await _queueService.PushAsync(new LoginFailedMessage
                {
                    Data = new CommonMessageData
                    {
                        Culture = CultureInfo.CurrentCulture.Name,
                        DeviceId = moreData?.DeviceId,
                        IP = _ip.ToString(),
                        UserId = login.Id,
                        UserName = login.Name,
                        OrganizationId = moreData?.OrganizationId,
                        TimeZone = rq.TimeZone,
                        TargetId = 0
                    },
                    Reason = loginResult.Type,
                    UserAgent = userAgent
                }, PlatformSharedContext.Default.LoginFailedMessage, cancellationToken);
            }

            return (loginResult, refreshToken);
        }

        private async Task<(ActionResult result, CurrentUser? user, TokenQueryData? data)> LoginAsync(CoreUserLogin user, string clientId, string deviceName, DeviceType deviceType, string region, int? organizationId, int? fromOrganizationId, IEnumerable<string>? authScopes, string? timezone, CancellationToken cancellationToken)
        {
            // Default values
            var culture = CultureInfo.CurrentCulture.Name;

            // Token user may pass 0 for organization id
            if (organizationId < 1) organizationId = null;
            if (fromOrganizationId < 1) fromOrganizationId = null;

            // Complete login with the SP
            var userIdSP = new NpgsqlParameter<int>("p_user_id", user.Id);
            var latestOrganizationIdSP = new NpgsqlParameter<int?>("p_latest_organization_id", user.LatestOrganizationIds?.FirstOrDefault());
            var organizationIdSP = new NpgsqlParameter<int?>("p_target_organization_id", organizationId);
            var fromOrganizationIdSP = new NpgsqlParameter<int?>("p_from_organization_id", fromOrganizationId);
            var deviceNameSP = new NpgsqlParameter<string>("p_device_name", deviceName);
            var deviceTypeSP = new NpgsqlParameter<short>("p_device_type", (byte)deviceType);
            var clientIdSP = new NpgsqlParameter<string>("p_client_id", clientId);
            var ipSP = new NpgsqlParameter<string>("p_ip", _ip.ToString());
            var cultureSP = new NpgsqlParameter<string>("p_culture", culture);
            var timezoneSP = new NpgsqlParameter<string?>("p_timezone", timezone);

            // IQuerable<T>.FirstOrDefault() adds SQL that filters by the first row number
            // Here we use the function to get the first row, not the filter
            // The returned columns naming should be the same as the model, otherwise EFCore.NamingConventions need to be used
            // CALL SP_NAME (stored procedure)
            var data = (await _db.Database.SqlQuery<CompleteLoginData>($"SELECT * FROM complete_login({userIdSP}, {latestOrganizationIdSP}, {organizationIdSP}, {fromOrganizationIdSP}, {deviceNameSP}, {deviceTypeSP}, {clientIdSP}, {ipSP}, {cultureSP}, {timezoneSP})")
                .ToListAsync(cancellationToken)).FirstOrDefault();

            if (data == null)
            {
                return (ApplicationErrors.DataProcessingFailed.AsResult(nameof(data)), null, null);
            }
            else if (organizationId.HasValue && !organizationId.Equals(data.OrganizationId))
            {
                // Required organization id is invalid
                return (ApplicationErrors.NoValidData.AsResult(nameof(organizationId)), null, null);
            }
            else if (fromOrganizationId.HasValue && !fromOrganizationId.Equals(data.ChannelOrganizationId))
            {
                // Required from organization id is invalid
                return (ApplicationErrors.NoValidData.AsResult(nameof(fromOrganizationId)), null, null);
            }
            else if (data.OrgStatus.HasValue && data.OrgStatus.Value > EntityStatus.Approved)
            {
                // User status is invalid
                return (ApplicationErrors.AccountDisabled.AsResult(nameof(data.OrgStatus)), null, null);
            }
            else if (data.OrgExpiry.HasValue && data.OrgExpiry.Value < DateTimeOffset.UtcNow)
            {
                // User expired
                return (ApplicationErrors.AccountExpired.AsResult(nameof(data.OrgExpiry)), null, null);
            }

            // Permission scopes
            var scopes = new List<string>
            {
                MyAppConstants.CoreApp
            };

            // Is super admin
            // Make sure it's not a partner organization and the user is manager or above
            if (App.Configuration.SuperAdminOrganizationId.Equals(data.OrganizationId)
                && data.ParentOrganizationId == null && data.ChannelOrganizationId == null
                && data.UserRole >= UserRole.Manager)
            {
                scopes.Add(MyAppConstants.SuperApp);
            }

            // App paid scopes
            scopes.AddRange(data.Scopes.Select(CurrentUser.AppIdToScope));

            // Limit scopes
            if (authScopes != null)
            {
                scopes = scopes.Intersect(authScopes).ToList();
            }

            // Token data
            var tokenData = new DeviceTokenData
            {
                Region = region,
                Scopes = scopes,
                Uid = data.Uid,
                OrganizationId = data.OrganizationId.GetValueOrDefault(),
                ParentOrganizationId = data.ParentOrganizationId,
                ChannelOrganizationId = data.ChannelOrganizationId,
            };

            // Create user
            var tokenQueryData = new TokenQueryData
            {
                DeviceId = data.DeviceId,
                UserId = user.Id,
                Culture = culture,
                Data = tokenData
            };
            var (result, tokenUser) = CreateUserFrom(tokenQueryData, new TokenQueryUser
            {
                Id = user.Id,
                Name = data.LocalName ?? user.Name,
                GivenName = user.GivenName,
                FamilyName = user.FamilyName,
                LatinGivenName = user.LatinGivenName,
                LatinFamilyName = user.LatinFamilyName,
                Avatar = data.LocalAvatar ?? user.Avatar,
                LatestAppId = user.LatestAppIds?.FirstOrDefault(),
                OrganizationName = data.OrganizationName,
                Oid = data.Oid,
                Role = data.UserRole
            }, timezone);

            if (tokenUser == null)
            {
                return (result, null, null);
            }

            return (result, tokenUser, tokenQueryData);
        }

        // Complete registration - 完成注册
        // Login with password - 使用密码登录
        // API Refresh token - 接口刷新令牌
        // Log in from OAuth2 client - 从OAuth2客户端登录
        // Sign up from OAuth2 client - 从OAuth2客户端注册
        private async Task<(ActionResult result, string? refreshToken, MoreData? moreData)> CompleteLoginAsync(CoreUserLogin user, string clientId, string deviceName, DeviceType deviceType, string region, int? organizationId, int? fromOrganizationId, AuthRequest? auth, string? timezone, CancellationToken cancellationToken)
        {
            var (result, tokenUser, data) = await LoginAsync(user, clientId, deviceName, deviceType, region, organizationId, fromOrganizationId, auth?.Scopes, timezone, cancellationToken);

            if (!result.Ok || tokenUser == null || data == null)
            {
                return (result, null, null);
            }

            var moreData = new MoreData
            {
                DeviceId = tokenUser.DeviceIdInt,
                OrganizationId = tokenUser.OrganizationInt > 0 ? tokenUser.OrganizationInt : null
            };

            if (auth == null)
            {
                var minutes = App.AuthService.AccessTokenMinutes;
                var accessToken = App.AuthService.CreateAccessToken(tokenUser, null, minutes);

                var tokenData = data.Data;

                // Refresh token
                var refreshToken = await CreateRefreshTokenAsync(user.Id, data.DeviceId, data.Culture, TokenResponseType.Token, tokenData, null, cancellationToken);

                // Serverside device id
                // Encrypt DeviceId for client identifier
                var deviceId = await HashEncryptAsync(data.DeviceId.ToString(), App.Configuration.InitCallEncryptionIdentifier);

                var publicData = new PublicUserData
                {
                    Name = user.Name,
                    GivenName = user.GivenName,
                    FamilyName = user.FamilyName,
                    LatinGivenName = user.LatinGivenName,
                    LatinFamilyName = user.LatinFamilyName,
                    Avatar = user.Avatar,
                    Organization = tokenData.OrganizationId,
                    IsChannel = tokenData.ChannelOrganizationId.HasValue,
                    IsParent = tokenData.ParentOrganizationId.HasValue,
                    Role = tokenUser.RoleValue,
                    TokenScheme = BearerTokenType,
                    Token = accessToken,
                    Seconds = 60 * minutes,
                    DeviceId = deviceId
                };

                // Save
                publicData.SaveTo(result);

                return (result, refreshToken, moreData);
            }
            else
            {
                var uri = await AuthRequestAsync(auth, tokenUser, data, cancellationToken);
                return (result, uri, moreData);
            }
        }

        /// <summary>
        /// Get log in URL
        /// 获取登录URL
        /// </summary>
        /// <param name="client">Auth client</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="deviceId">Region (like CN) & Device id</param>
        /// <returns>Result</returns>
        public IResult GetLogInUrl(IAuthClient client, string? userAgent, string deviceId)
        {
            if (!this.CheckDevice(userAgent, deviceId[2..], out var result, out _))
            {
                return Results.BadRequest(result);
            }
            else
            {
                return Results.Content(client.GetLogInUrl(deviceId), "text/plain");
            }
        }

        /// <summary>
        /// Get sign up URL
        /// 获取注册URL
        /// </summary>
        /// <param name="client">Auth client</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="deviceId">Region (like CN) & Device id</param>
        /// <returns>Result</returns>
        public IResult GetSignUpUrl(IAuthClient client, string? userAgent, string deviceId)
        {
            if (!this.CheckDevice(userAgent, deviceId[2..], out var result, out _))
            {
                return Results.BadRequest(result);
            }
            else
            {
                return Results.Content(client.GetSignUpUrl(deviceId), "text/plain");
            }
        }

        /// <summary>
        /// Log in from OAuth2 client
        /// 从OAuth2客户端登录
        /// </summary>
        /// <param name="client">OAuth2 client</param>
        /// <param name="type">Auth type</param>
        /// <param name="context">OAuth2 Request HTTPContext</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async ValueTask LogInAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default)
        {
            var (result, userInfo, loginData) = await this.GetUserInfoAsync(client, context, cancellationToken);

            if (result.Ok && userInfo != null && loginData != null)
            {
                var loginUser = await ReadUserAsync(type, userInfo.OpenId);
                if (loginUser == null)
                {
                    var url = $"{App.Configuration.AuthFailureUrl}?type={type}";
                    context.Response.Redirect(url, true);
                }
                else
                {
                    await ValidateUserAsync(context, type, loginUser, loginData, cancellationToken);
                }
            }
            else
            {
                Logger.LogError("Log in failed: {result}", result);
                RedirectToFailureUrl(context.Response, type, result.Title ?? "Log in failed", result.Type, result.Field);
            }
        }

        /// <summary>
        /// Login id check
        /// 登录编号检索
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> LoginIdAsync(LoginIdRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            // Check device
            if (!this.CheckDevice(userAgent, rq.DeviceId, out var checkResult, out var cd))
            {
                return checkResult;
            }
            var deviceCore = cd.Value.DeviceCore;

            try
            {
                var id = DecryptDeviceData(rq.Id, deviceCore);
                if (string.IsNullOrEmpty(id) || id.Length < 6)
                {
                    return ApplicationErrors.NoValidData.AsResult();
                }
                var (result, _) = await LoginIdAsync(id, rq.Region, rq.TimeZone, cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "LoginIdAsync failed");
                return ApplicationErrors.DataProcessingFailed.AsResult();
            }
        }

        /// <summary>
        /// Login id check
        /// 登录编号检索
        /// </summary>
        /// <param name="id">Email or mobile</param>
        /// <param name="region">Country or region id</param>
        /// <param name="timezone">Time zone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple results</returns>
        public async ValueTask<(IActionResult, LoginUserWithPassword?)> LoginIdAsync(string id, string region, string timezone, CancellationToken cancellationToken = default)
        {
            // Check
            var type = id.Contains('@') ? CoreUserIdentifierType.Email : CoreUserIdentifierType.Mobile;
            var result = CheckId(type, ref id, region);
            if (!result.Ok)
            {
                return (result, null);
            }

            // Login with id check
            var data = await _db.CoreUserIdentifiers
                .AsNoTracking()
                .Where(i => i.Type == type && i.Value == id)
                .Select(i => new LoginUserWithPassword { Id = i.CoreUser.Id, Name = i.CoreUser.Name, Password = i.CoreUser.Password, Status = i.CoreUser.Status, FrozenTime = i.CoreUser.FrozenTime, Step = i.CoreUser.Step })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return (ApplicationErrors.NoUserFound.AsResult(), null);
            }

            result = data.ValidateUser(TimeZoneUtils.GetTimeZoneBase(timezone));
            if (!result.Ok)
            {
                return (result, null);
            }

            if (data.Step != 0)
            {
                result.Data.Add("Step", data.Step);
            }

            return (result, data);
        }

        private async Task<LoginUser?> ReadUserAsync(CoreUserIdentifierType type, string openId)
        {
            return await _db.CoreUserIdentifiers.AsNoTracking().Where(i => i.Type == type && i.Value == openId)
                .Select(i => new LoginUser { Id = i.CoreUser.Id, Status = i.CoreUser.Status, FrozenTime = i.CoreUser.FrozenTime, Step = i.CoreUser.Step, IdentifierId = i.Id })
                .FirstOrDefaultAsync();
        }

        private void RedirectToFailureUrl(HttpResponse response, CoreUserIdentifierType type, string error, string? errorType = null, string? errorField = null)
        {
            var url = $"{App.Configuration.AuthFailureUrl}?type={type}&error={HttpUtility.UrlEncode(error)}&errorType={HttpUtility.UrlEncode(errorType)}&errorField={HttpUtility.UrlEncode(errorField)}";
            response.Redirect(url, true);
        }

        private bool TokenDataEquals(DeviceTokenData source, DeviceTokenData data)
        {
            return source.Scopes.Count == data.Scopes.Count && source.Scopes.Order().Equals(data.Scopes.Order())
                && source.Region == data.Region
                && source.OrganizationId == data.OrganizationId
                && source.ParentOrganizationId == data.ParentOrganizationId
                && source.ChannelOrganizationId == data.ChannelOrganizationId
            ;
        }

        private async Task<string> CreateRefreshTokenAsync(int userId, int deviceId, string culture, TokenResponseType responseType, DeviceTokenData data, int? appId, CancellationToken cancellationToken)
        {
            // Use the latest token to avoid huge creation of new one
            var tokenData = await _db.CoreUserDeviceTokens
                .AsNoTracking()
                .Where(t => t.DeviceId == deviceId && t.ResponseType == responseType && t.AppId == appId)
                .OrderByDescending(t => t.Expiry)
                .Select(t => new { t.Id, t.Token, t.Expiry, t.Data })
                .FirstOrDefaultAsync(cancellationToken);

            // Remove the latest expiry one
            if (tokenData != null)
            {
                if (tokenData.Expiry < DateTime.UtcNow)
                {
                    await _db.CoreUserDeviceTokens.Where(t => t.Id == tokenData.Id).ExecuteDeleteAsync(cancellationToken);
                    tokenData = null;
                }
                else if (!TokenDataEquals(tokenData.Data, data))
                {
                    // New refresh token is required
                    tokenData = null;
                }
            }

            // Refresh token / code expiry
            var expiry = responseType == TokenResponseType.Token
                ? DateTime.UtcNow.AddDays(App.Configuration.RefreshTokenDays).ToSqlDateTime()
                : DateTime.UtcNow.AddMinutes(3).ToSqlDateTime();

            string token;
            if (tokenData == null)
            {
                // Random token hashed
                token = await App.HashPasswordAsync($"{userId}-{deviceId}-{Guid.NewGuid()}-{CryptographyUtils.CreateRandString(RandStringKind.All, 6)}");

                // Create token
                await _db.CoreUserDeviceTokens.AddAsync(new CoreUserDeviceToken
                {
                    DeviceId = deviceId,
                    AppId = appId,
                    Token = token,
                    Expiry = expiry,
                    Data = data,
                    Culture = culture,
                    ResponseType = responseType
                }, cancellationToken);

                // Save it
                await _db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                token = tokenData.Token;

                // Update the expiry
                await _db.CoreUserDeviceTokens.Where(t => t.Id == tokenData.Id).ExecuteUpdateAsync(t => t.SetProperty(t => t.Expiry, expiry), cancellationToken);
            }

            // Return the token
            return token;
        }

        /// <summary>
        /// Refresh token
        /// 刷新令牌
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<(IActionResult result, string? newRefreshToken)> RefreshTokenAsync(RefreshTokenData data, CancellationToken cancellationToken = default)
        {
            // Check device
            if (!this.CheckDevice(data.UserAgent, data.DeviceId, out var checkResult, out var cd))
            {
                return (checkResult, null);
            }

            var deviceCore = cd.Value.DeviceCore;

            var token = DecryptDeviceData(data.Token, deviceCore);
            if (string.IsNullOrEmpty(token))
            {
                return (ApplicationErrors.NoValidData.AsResult("Token"), null);
            }

            var deviceName = cd.Value.Parser.ToShortName();

            var tokenData = await _db.CoreUserDeviceTokens
                .AsNoTracking()
                .Where(t => t.ResponseType == TokenResponseType.Token && t.AppId == null && t.Token == token)
                .Select(t => new
                {
                    t.Id,
                    t.Expiry,
                    CoreUser = new CoreUserLogin
                    {
                        Id = t.Device.CoreUser.Id,
                        Name = t.Device.CoreUser.Name,
                        GivenName = t.Device.CoreUser.GivenName,
                        FamilyName = t.Device.CoreUser.FamilyName,
                        LatinGivenName = t.Device.CoreUser.LatinGivenName,
                        LatinFamilyName = t.Device.CoreUser.LatinFamilyName,
                        Avatar = t.Device.CoreUser.Avatar,
                        LatestOrganizationIds = t.Device.CoreUser.LatestOrganizationIds,
                        LatestAppIds = t.Device.CoreUser.LatestAppIds
                    },
                    t.Device.DeviceType,
                    Data = new TokenQueryData
                    {
                        DeviceId = t.DeviceId,
                        UserId = t.Device.CoreUser.Id,
                        Culture = t.Culture,
                        Data = t.Data
                    }
                }).FirstOrDefaultAsync(cancellationToken);

            if (tokenData == null)
            {
                return (ApplicationErrors.TokenExpired.AsResult("Id"), null);
            }
            else if (tokenData.Expiry < DateTime.UtcNow)
            {
                // Remove the token
                await _db.CoreUserDeviceTokens.Where(t => t.Id == tokenData.Id).ExecuteDeleteAsync(cancellationToken);

                return (ApplicationErrors.TokenExpired.AsResult(), null);
            }

            var user = tokenData.CoreUser;
            var td = tokenData.Data.Data;

            var (loginResult, refreshToken, _) = await CompleteLoginAsync(user, data.DeviceId, deviceName, tokenData.DeviceType, td.Region, td.OrganizationId, td.ChannelOrganizationId ?? td.ParentOrganizationId, null, data.TimeZone, cancellationToken);

            return (loginResult, refreshToken);
        }

        private async Task ValidateUserAsync(HttpContext context, CoreUserIdentifierType type, LoginUser user, AuthLoginValidateData loginData, CancellationToken cancellationToken)
        {
            var result = user.ValidateUser(null);
            if (result.Ok)
            {
                if (user.Step > 0)
                {
                    var token = CreateRegistrationToken(user.Id);
                    var url = $"{App.Configuration.AuthRegistrationUrl}{user.Step}?token={HttpUtility.UrlEncode(token)}";
                    context.Response.Redirect(url, true);
                }
                else
                {
                    await ValidateUserToLoginAsync(context, type, user, loginData, cancellationToken);
                }
            }
            else
            {
                RedirectToFailureUrl(context.Response, type, result.Title ?? "Validate User Error", result.Type);
            }
        }

        private async Task ValidateUserToLoginAsync(HttpContext context, CoreUserIdentifierType type, LoginUser loginUser, AuthLoginValidateData loginData, CancellationToken cancellationToken)
        {
            var user = await _db.CoreUsers.AsNoTracking()
                .Where(u => u.Id == loginUser.Id)
                .Select(u => new CoreUserLogin
                {
                    Id = u.Id,
                    Name = u.Name,
                    GivenName = u.GivenName,
                    FamilyName = u.FamilyName,
                    LatinGivenName = u.LatinGivenName,
                    LatinFamilyName = u.LatinFamilyName,
                    Avatar = u.Avatar,
                    LatestOrganizationIds = u.LatestOrganizationIds,
                    LatestAppIds = u.LatestAppIds
                }).FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                RedirectToFailureUrl(context.Response, type, "No User Found");
                return;
            }

            var deviceName = loginData.Parser.ToShortName();

            // No authorization request, will be done in the client side
            var (result, refreshToken, _) = await CompleteLoginAsync(user, loginData.DeviceId, deviceName, DeviceType.Web, loginData.Region, null, null, null, null, cancellationToken);

            var jsonResult = JsonSerializer.Serialize(result, CommonJsonSerializerContext.Default.ActionResult);

            context.Response.Redirect($"{App.Configuration.AuthSuccessUrl}?result={HttpUtility.UrlEncode(jsonResult)}&token={HttpUtility.UrlEncode(refreshToken)}", true);
        }

        /// <summary>
        /// Sign up from OAuth2 client
        /// 从OAuth2客户端注册
        /// </summary>
        /// <param name="client">OAuth2 client</param>
        /// <param name="type">Auth type</param>
        /// <param name="context">OAuth2 Request HTTPContext</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async ValueTask SignUpAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default)
        {
            var (result, userInfo, loginData) = await this.GetUserInfoAsync(client, context, cancellationToken);

            if (result.Ok && userInfo != null && loginData != null)
            {
                var loginUser = await ReadUserAsync(type, userInfo.OpenId);
                if (loginUser == null)
                {
                    // Has email
                    var hasEmail = false;
                    if (!string.IsNullOrEmpty(userInfo.Email) && userInfo.EmailVerified is true)
                    {
                        // Check email exists
                        loginUser = await ReadUserAsync(CoreUserIdentifierType.Email, userInfo.Email);
                        if (loginUser != null)
                        {
                            result = loginUser.ValidateUser(null);
                            if (result.Ok)
                            {
                                // Current user
                                var currentUser = await _db.CoreUsers.FindAsync([loginUser.Id], cancellationToken);
                                if (currentUser == null)
                                {
                                    return;
                                }

                                // Update the user
                                if (string.IsNullOrEmpty(currentUser.FamilyName))
                                    currentUser.FamilyName = userInfo.FamilyName;
                                if (string.IsNullOrEmpty(currentUser.GivenName))
                                    currentUser.GivenName = userInfo.GivenName;

                                // Ignore the avator update to avoid downloading

                                // Current oauth data
                                currentUser.CoreUserIdentifiers.Add(new CoreUserIdentifier
                                {
                                    Type = type,
                                    Value = userInfo.OpenId,
                                });

                                _db.CoreUsers.Update(currentUser);

                                // Update the email reference
                                await _db.CoreUserIdentifiers.Where(i => i.CoreUserId == loginUser.Id && i.Id == loginUser.IdentifierId)
                                    .ExecuteUpdateAsync(i => i.SetProperty(i => i.RefType, type), cancellationToken);

                                // Save
                                await _db.SaveChangesAsync(cancellationToken);

                                // Ready for login
                                await ValidateUserToLoginAsync(context, type, loginUser, loginData, cancellationToken);

                                return;
                            }
                            else
                            {
                                RedirectToFailureUrl(context.Response, type, result.Title ?? "Account Invalid", result.Type, result.Field);
                            }

                            return;
                        }

                        hasEmail = true;
                    }

                    string? avatar = null;
                    if (!string.IsNullOrEmpty(userInfo.Picture))
                    {
                        // Download avatar
                        try
                        {
                            using var response = await _httpClientFactory.CreateClient().GetAsync(userInfo.Picture, cancellationToken);
                            response.EnsureSuccessStatusCode();
                            var ext = MimeTypeMap.TryGetExtension(response.Content.Headers.ContentType?.MediaType) ?? Path.GetExtension(userInfo.Picture);
                            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                            if (stream != null)
                            {
                                var avatarPath = $"/UserAvatars/{DateTime.UtcNow:yyyyMM}/{userInfo.OpenId}{ext}";
                                var tags = new Dictionary<string, string>() { { "AuthType", type.ToString() }, { "UserId", userInfo.OpenId } };
                                var saveResult = await _storage.WriteAsync(avatarPath, stream, WriteCase.CreateNew, tags, cancellationToken);
                                if (saveResult)
                                {
                                    avatar = _storage.GetUrl(avatarPath);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Only log the exception, no block for execution
                            Logger.LogError(ex, "Download avatar failed: {url}", userInfo.Picture);
                        }
                    }

                    // Basic data
                    var user = new CoreUser
                    {
                        Name = userInfo.Name,
                        FamilyName = userInfo.FamilyName,
                        GivenName = userInfo.GivenName,
                        Avatar = avatar,
                        Step = hasEmail ? CoreUserStep.Email : CoreUserStep.OAuth
                    };

                    // Current oauth data
                    user.CoreUserIdentifiers.Add(new CoreUserIdentifier
                    {
                        Type = type,
                        Value = userInfo.OpenId,
                    });

                    if (hasEmail)
                    {
                        // Additional email data
                        user.CoreUserIdentifiers.Add(new CoreUserIdentifier
                        {
                            Type = CoreUserIdentifierType.Email,
                            Value = userInfo.Email!,
                            RefType = type
                        });
                    }

                    // AddAsync vs Add
                    await _db.CoreUsers.AddAsync(user, cancellationToken);

                    await _db.SaveChangesAsync(cancellationToken);

                    await ValidateUserAsync(context, type, new LoginUser { Id = user.Id, Status = user.Status, FrozenTime = user.FrozenTime, Step = user.Step }, loginData, cancellationToken);
                }
                else
                {
                    await ValidateUserAsync(context, type, loginUser, loginData, cancellationToken);
                }
            }
            else
            {
                Logger.LogError("Sign up failed: @{result}", result);
                RedirectToFailureUrl(context.Response, type, result.Title ?? "Sign up failed", result.Type, result.Field);
            }
        }

        /// <summary>
        /// Web init call
        /// Web初始化调用
        /// </summary>
        /// <param name="rq">Rquest data</param>
        /// <param name="identifier">User identifier</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> WebInitCallAsync(InitCallRQ rq, string identifier)
        {
            // Init call
            return await InitCallAsync(rq, identifier);
        }

        /// <summary>
        /// Async init call update
        /// 异步初始化调用更新
        /// </summary>
        /// <param name="prevDeviceId">Previous client device id</param>
        /// <param name="newDeviceId">New client device id</param>
        /// <param name="deviceId">Serverside device id</param>
        /// <returns>Task</returns>
        protected override async Task InitCallUpdateAsync(string prevDeviceId, string newDeviceId, int deviceId, CancellationToken cancellationToken = default)
        {
            await _db.CoreUserDevices.Where(d => d.Id == deviceId && d.ClientId.Equals(prevDeviceId))
                .ExecuteUpdateAsync(d => d.SetProperty(d => d.ClientId, newDeviceId), cancellationToken);
        }

        /// <summary>
        /// View register data
        /// 浏览注册数据
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<RegisterUserData?> ViewRegisterDataAsync(CancellationToken cancellationToken = default)
        {
            if (_regUser == null) return null;

            var user = await _db.CoreUsers
                .AsNoTracking()
                .Where(u => u.Id == _regUser.IdInt)
                .Select(u => new RegisterUserData { Name = u.Name })
                .FirstOrDefaultAsync(cancellationToken);

            return user;
        }



        /// <summary>
        /// OAuth create token from code
        /// OAuth从代码创建令牌
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<AppTokenData?> OAuthCreateTokenAsync(AuthCreateTokenRQ rq, CancellationToken cancellationToken = default)
        {
            // Check app secret
            var appData = await AuthGetAppSecretAsync(rq.AppId, rq.AppKey, cancellationToken);

            if (appData == null)
            {
                Logger.LogWarning("App secret not found: {AppId}, {AppKey}", rq.AppId, rq.AppKey);
                return null;
            }

            // Check signature
            var expectedSignature = rq.SignWith(appData.AppSecret);
            if (rq.Sign != expectedSignature)
            {
                Logger.LogWarning("Signature not match: {Sign}, {ExpectedSignature}", rq.Sign, expectedSignature);
                return null;
            }

            var data = await _db.CoreUserDeviceTokens
                .AsNoTracking()
                .Where(d => d.ResponseType == TokenResponseType.Code && d.AppId == rq.AppId && d.Token == rq.Code && d.Expiry >= DateTime.UtcNow)
                .Select(d => new TokenQueryData
                {
                    DeviceId = d.DeviceId,
                    UserId = d.Device.CoreUserId,
                    Culture = d.Culture,
                    Data = d.Data
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                Logger.LogWarning("Token query data not found");
                return null;
            }

            // Check redirect uri
            if (!rq.RedirectUri.Equals(data.Data.RedirectUri))
            {
                Logger.LogWarning("Redirect uri not match: {RedirectUri}, {DataRedirectUri}", rq.RedirectUri, data.Data.RedirectUri);
                return null;
            }

            // User
            var (result, user) = await CreateUserFromQueryDataAsync(data, null, cancellationToken);
            if (user == null)
            {
                Logger.LogWarning("User not found with @{data}, @{result}", data, result);
                return null;
            }

            return await CreateAppTokenDataAsync(user, rq.AppId, appData.AppSecret, data.Data.AccessType == AuthRequest.OfflineAccessType, data, cancellationToken);
        }

        private async Task<(ActionResult, CurrentUser?)> CreateUserFromQueryDataAsync(TokenQueryData data, string? timezone, CancellationToken cancellationToken)
        {
            var userData = await _db.CoreUsers
                .AsNoTracking()
                .Where(u => u.Id == data.UserId)
                .GroupJoin(_db.CoreOrganizationUsers, u => u.Id, ou => ou.CoreUserId, (u, ou) => new { u, ou })
                .SelectMany(d => d.ou.Where(ou => ou.CoreOrganizationId == data.Data.OrganizationId).DefaultIfEmpty(), (d, ou) => new TokenQueryUser
                {
                    Id = d.u.Id,
                    GivenName = d.u.GivenName,
                    FamilyName = d.u.FamilyName,
                    LatinGivenName = d.u.LatinGivenName,
                    LatinFamilyName = d.u.LatinFamilyName,
                    Status = d.u.Status,
                    FrozenTime = d.u.FrozenTime,
                    Step = d.u.Step,
                    LatestAppId = d.u.LatestAppIds == null ? null : d.u.LatestAppIds.FirstOrDefault(),
                    OrgStatus = ou == null ? null : ou.Status,
                    OrgExpiry = ou == null ? null : ou.Expiry,
                    Name = ou == null ? d.u.Name : (ou.LocalName ?? d.u.Name),
                    Avatar = ou == null ? d.u.Avatar : (ou.LocalAvatar ?? d.u.Avatar),
                    Role = ou == null ? null : ou.UserRole,
                    Oid = ou == null ? null : ou.Id,
                    OrganizationName = ou == null ? null : ou.CoreOrganization.Name
                })
                .FirstOrDefaultAsync(cancellationToken);

            return CreateUserFrom(data, userData, timezone);
        }

        private (ActionResult, CurrentUser?) CreateUserFrom(TokenQueryData data, TokenQueryUser? userData, string? timezone)
        {
            if (userData == null)
            {
                return (ApplicationErrors.NoUserFound.AsResult(), null);
            }

            // Time zone info
            var tz = TimeZoneUtils.GetTimeZoneBase(timezone);

            var result = userData.ValidateUser(tz);
            if (!result.Ok)
            {
                return (result, null);
            }
            else if (userData.Step > 0)
            {
                return (ApplicationErrors.DataNotReady.AsResult("Step"), null);
            }

            var user = new CurrentUser
            {
                Id = userData.Id.ToString(),
                Name = userData.Name,
                GivenName = userData.GivenName,
                FamilyName = userData.FamilyName,
                LatinGivenName = userData.LatinGivenName,
                LatinFamilyName = userData.LatinFamilyName,
                Avatar = userData.Avatar,
                DeviceId = data.DeviceId.ToString(),
                Organization = data.Data.OrganizationId.ToString(),
                OrganizationName = userData.OrganizationName,
                ParentOrganization = data.Data.ParentOrganizationId?.ToString(),
                ChannelOrganization = data.Data.ChannelOrganizationId?.ToString(),
                Oid = userData.Oid.GetValueOrDefault().ToString(),
                Uid = data.Data.Uid.ToString(),
                RoleValue = (short)userData.Role.GetValueOrDefault(UserRole.User),
                AppId = userData.LatestAppId,
                Scopes = data.Data.Scopes,
                ClientIp = _ip,
                Language = new CultureInfo(data.Culture),
                Region = data.Data.Region,
                TimeZone = tz
            };

            return (result, user);
        }

        /// <summary>
        /// OAuth refresh token
        /// OAuth刷新令牌
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<AppTokenData?> OAuthRefreshTokenAsync(AuthRefreshTokenRQ rq, CancellationToken cancellationToken = default)
        {
            // Check app secret
            var appData = await AuthGetAppSecretAsync(rq.AppId, rq.AppKey, cancellationToken);

            if (appData == null)
            {
                Logger.LogWarning("App secret not found: {AppId}, {AppKey}", rq.AppId, rq.AppKey);
                return null;
            }

            // Check signature
            var expectedSignature = rq.SignWith(appData.AppSecret);
            if (rq.Sign != expectedSignature)
            {
                Logger.LogWarning("Signature not match: {Sign}, {ExpectedSignature}", rq.Sign, expectedSignature);
                return null;
            }

            return await CreateAppTokenDataAsync(rq.AppId, rq.RefreshToken, appData.AppSecret, rq.TimeZone, cancellationToken);
        }

        /// <summary>
        /// OAuth refresh token result
        /// OAuth刷新令牌结果
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<(IActionResult result, string? newRefreshToken)> OAuthRefreshTokenResultAsync(AuthRefreshTokenRQ rq, CancellationToken cancellationToken = default)
        {
            // Check app secret
            var appData = await AuthGetAppSecretAsync(rq.AppId, rq.AppKey, cancellationToken);

            if (appData == null)
            {
                Logger.LogWarning("App secret not found: {AppId}, {AppKey}", rq.AppId, rq.AppKey);
                return (ApplicationErrors.NoValidData.AsResult("AppId"), null);
            }

            // Check signature
            var expectedSignature = rq.SignWith(appData.AppSecret);
            if (rq.Sign != expectedSignature)
            {
                Logger.LogWarning("Signature not match: {Sign}, {ExpectedSignature}", rq.Sign, expectedSignature);
                return (ApplicationErrors.NoValidData.AsResult("Sign"), null);
            }

            var tokenData = await _db.CoreUserDeviceTokens
                .Where(d => d.ResponseType == TokenResponseType.Token && d.AppId == rq.AppId && d.Token == rq.RefreshToken)
                .Select(d => new
                {
                    d.Id,
                    d.Expiry,
                    CoreUser = new CoreUserLogin
                    {
                        Id = d.Device.CoreUser.Id,
                        Name = d.Device.CoreUser.Name,
                        GivenName = d.Device.CoreUser.GivenName,
                        FamilyName = d.Device.CoreUser.FamilyName,
                        LatinGivenName = d.Device.CoreUser.LatinGivenName,
                        LatinFamilyName = d.Device.CoreUser.LatinFamilyName,
                        Avatar = d.Device.CoreUser.Avatar,
                        LatestOrganizationIds = d.Device.CoreUser.LatestOrganizationIds,
                        LatestAppIds = d.Device.CoreUser.LatestAppIds
                    },
                    d.Device.DeviceType,
                    d.Device.Name,
                    d.Device.ClientId,
                    Data = new TokenQueryData
                    {
                        DeviceId = d.DeviceId,
                        UserId = d.Device.CoreUserId,
                        Culture = d.Culture,
                        Data = d.Data
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (tokenData == null)
            {
                Logger.LogWarning("Token query data not found");
                return (ApplicationErrors.NoValidData.AsResult("Token"), null);
            }
            else if (tokenData.Expiry < DateTime.UtcNow)
            {
                // Remove the token
                await _db.CoreUserDeviceTokens.Where(t => t.Id == tokenData.Id).ExecuteDeleteAsync(cancellationToken);

                Logger.LogWarning("Token expired: {Expiry}", tokenData.Expiry);
                return (ApplicationErrors.TokenExpired.AsResult(), null);
            }

            var data = tokenData.Data;
            var td = data.Data;

            var (loginResult, refreshToken, _) = await CompleteLoginAsync(tokenData.CoreUser, tokenData.ClientId, tokenData.Name, tokenData.DeviceType, td.Region, td.OrganizationId, td.ChannelOrganizationId ?? td.ParentOrganizationId, null, null, cancellationToken);

            return (loginResult, refreshToken);
        }

        /// <summary>
        /// Get OAuth user info
        /// 获取OAuth用户信息
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task OAuthUserInfoAsync(HttpResponse? response, CancellationToken cancellationToken = default)
        {
            if (response == null || User == null) return;

            await response.WriteAsJsonAsync(User, ModelJsonSerializerContext.Default.CurrentUser, null, cancellationToken);
        }

        /// <summary>
        /// Switch organizations
        /// 切换机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<AppTokenData?> SwitchOrgAsync(SwitchOrgProxyRQ rq, CancellationToken cancellationToken = default)
        {
            if (User == null)
            {
                return null;
            }

            // Check app secret
            var appData = await AuthGetAppSecretAsync(rq.AppId, rq.AppKey, cancellationToken);

            if (appData == null)
            {
                Logger.LogWarning("App secret not found: {AppId}, {AppKey}", rq.AppId, rq.AppKey);
                return null;
            }

            // Check signature
            var expectedSignature = rq.SignWith(appData.AppSecret);
            if (rq.Sign != expectedSignature)
            {
                Logger.LogWarning("Signature not match: {Sign}, {ExpectedSignature}", rq.Sign, expectedSignature);
                return null;
            }

            var user = await _db.CoreUsers.AsNoTracking()
                .Where(u => u.Id == User.IdInt)
                .Select(u => new CoreUserLogin
                {
                    Id = u.Id,
                    Name = u.Name,
                    GivenName = u.GivenName,
                    FamilyName = u.FamilyName,
                    LatinGivenName = u.LatinGivenName,
                    LatinFamilyName = u.LatinFamilyName,
                    Avatar = u.Avatar,
                    LatestOrganizationIds = u.LatestOrganizationIds,
                    LatestAppIds = u.LatestAppIds
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                Logger.LogWarning("User {user} not found", User.Id);
                return null;
            }

            var device = await _db.CoreUserDevices.AsNoTracking()
                .Where(d => d.Id == User.DeviceIdInt)
                .Select(d => new
                {
                    d.DeviceType,
                    d.Name,
                    d.ClientId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (device == null)
            {
                Logger.LogWarning("Device {device} of {user} not found", User.DeviceId, User.Id);
                return null;
            }

            // Login
            var (result, tokenUser, data) = await LoginAsync(user, device.ClientId, device.Name, device.DeviceType, User.Region, rq.OrganizationId, rq.FromOrganizationId, User.Scopes, null, cancellationToken);

            if (!result.Ok || tokenUser == null || data == null)
            {
                Logger.LogWarning("Login failed: {result}, {tokenUser}, {data}", result, tokenUser, data);
                return null;
            }

            return await CreateAppTokenDataAsync(tokenUser, rq.AppId, appData.AppSecret, true, data, cancellationToken);
        }

        /// <summary>
        /// Reset password
        /// 重置密码
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> ResetPasswordAsync(ResetPasswordRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            if (_regUser == null)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Check device
            if (!this.CheckDevice(userAgent, rq.DeviceId, out var checkResult, out var cd))
            {
                return checkResult;
            }

            var deviceCore = cd.Value.DeviceCore;

            try
            {
                // Decrypt
                var id = DecryptDeviceData(rq.Id, deviceCore);
                if (string.IsNullOrEmpty(id))
                {
                    return ApplicationErrors.NoValidData.AsResult("Id");
                }

                var password = DecryptDeviceData(rq.Password, deviceCore);
                if (string.IsNullOrEmpty(password))
                {
                    return ApplicationErrors.NoValidData.AsResult("Password");
                }

                var (result, user) = await LoginIdAsync(id, rq.Region, rq.TimeZone, cancellationToken);
                if (!result.Ok || user == null)
                {
                    return result;
                }

                // Update password
                var newPassword = await App.HashPasswordAsync(_regUser.Id + password);
                await _db.CoreUsers.AsNoTracking().Where(u => u.Id == _regUser.IdInt)
                    .ExecuteUpdateAsync(u => u.SetProperty(u => u.Password, newPassword), cancellationToken);

                // Log
                await _queueService.PushAsync(new ResetPasswordMessage
                {
                    Data = new CommonMessageData
                    {
                        Culture = CultureInfo.CurrentCulture.Name,
                        DeviceId = null,
                        IP = _ip.ToString(),
                        UserId = user.Id,
                        UserName = user.Name,
                        OrganizationId = null,
                        TimeZone = rq.TimeZone,
                        TargetId = 0
                    },
                    UserAgent = userAgent
                }, PlatformSharedContext.Default.ResetPasswordMessage, cancellationToken);

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                return LogException(ex);
            }
        }

        /// <summary>
        /// Sign out
        /// 退出
        /// </summary>
        /// <param name="token">Refresh token</param>
        /// <returns>Task</returns>
        public async ValueTask<IActionResult> SignoutAsync(string token)
        {
            if (User == null)
            {
                return ApplicationErrors.NoUserFound.AsResult();
            }

            var result = await _db.CoreUserDeviceTokens.Where(t => t.DeviceId == User.DeviceIdInt && t.Token == token).ExecuteDeleteAsync();

            if (result == 0)
            {
                return ApplicationErrors.NoValidData.AsResult("Token");
            }
            else
            {
                return ActionResult.Success;
            }
        }

        /// <summary>
        /// Create registration access token
        /// 创建注册访问令牌
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Result</returns>
        private string CreateRegistrationToken(int id)
        {
            var user = new MinUserToken
            {
                Id = id.ToString(),
                Scopes = ["core"]
            };

            return App.AuthService.CreateAccessToken(user, MyAppConstants.RegistrationTokenAudience, 60);
        }

        /// <summary>
        /// Validate password callback
        /// 验证密码找回
        /// </summary>
        /// <returns>Task</returns>
        public async Task<ActionResult> ValidateCallbackAsync(CoreUserIdentifier identifier, CancellationToken cancellationToken = default)
        {
            // Find the user
            var user = await _db.CoreUsers
                .Where(u => u.CoreUserIdentifiers.Any(i => i.Type == identifier.Type && i.Value == identifier.Value))
                .Select(u => new { u.Id, u.Step })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return ApplicationErrors.NoUserMatch.AsResult();
            }
            else if (user.Step > 0)
            {
                return ApplicationErrors.InvalidAction.AsResult("Step");
            }

            var result = ActionResult.Success;

            result.Data["token"] = CreateRegistrationToken(user.Id);

            return result;
        }

        /// <summary>
        /// Validate registration
        /// 验证注册
        /// </summary>
        /// <returns>Task</returns>
        private async Task<ActionResult> ValidateRegistrationAsync(CoreUserIdentifier identifier, short step, ActionResult existError, CancellationToken cancellationToken = default)
        {
            int userId;

            if (_regUser == null)
            {
                // Find the user
                var data = await _db.CoreUsers.AsNoTracking()
                    .Where(u => u.CoreUserIdentifiers.Any(i => i.Type == identifier.Type && i.Value == identifier.Value))
                    .Select(u => new { u.Id, u.Step }).FirstOrDefaultAsync(cancellationToken);

                if (data == null)
                {
                    // Clear changes
                    _db.ChangeTracker.Clear();

                    // New user
                    var user = new CoreUser
                    {
                        Name = string.Empty,
                        Step = step
                    };

                    user.CoreUserIdentifiers.Add(identifier);

                    // AddAsync vs Add
                    _db.CoreUsers.Add(user);
                    await _db.SaveChangesAsync(cancellationToken);

                    userId = user.Id;
                }
                else if (data.Step == 0)
                {
                    // Registered
                    // Not secure to return the user with one time code
                    return existError;
                }
                else
                {
                    // Continue to register
                    userId = data.Id;
                }
            }
            else
            {
                // Update the user
                var user = await _db.CoreUsers.Where(u => u.Id == _regUser.IdInt)
                    .Include(u => u.CoreUserIdentifiers.Where(i => i.Type == identifier.Type))
                    .Select(u => new CoreUser
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Step = u.Step,
                        CoreUserIdentifiers = u.CoreUserIdentifiers
                    })
                    .FirstOrDefaultAsync(cancellationToken);
                if (user == null)
                {
                    return ApplicationErrors.NoValidData.AsResult("User");
                }

                if (!user.CoreUserIdentifiers.Any(i => i.Type.Equals(identifier.Type) && i.Value.Equals(identifier.Value)))
                {
                    // New identifier, check global exists or not
                    if (await _db.CoreUserIdentifiers.AnyAsync(i => i.Type == identifier.Type && i.Value == identifier.Value, cancellationToken))
                    {
                        return existError;
                    }

                    user.CoreUserIdentifiers.Add(identifier);
                }

                user.Step = CoreUserStep.Email;

                _db.CoreUsers.Update(user);
                await _db.SaveChangesAsync(cancellationToken);

                userId = 0;
            }

            var result = ActionResult.Success;

            if (userId > 0)
            {
                result.Data["token"] = CreateRegistrationToken(userId);
            }

            return result;
        }

        /// <summary>
        /// Validate email callback password code
        /// 验证电子邮箱找回密码代码
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> ValidateEmailCallbackAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            var (result, data) = _authCodeService.CreateValidateCodeData(rq, userAgent);
            if (!result.Ok || data == null)
            {
                return result;
            }

            var (resultValidate, resultData) = await _authCodeService.ValidateAsync(AuthCodeAction.UserCallbackEmailCode, data, cancellationToken);
            if (!resultValidate.Ok || resultData == null)
            {
                return resultValidate;
            }

            // Identifier
            var identifier = new CoreUserIdentifier
            {
                Type = CoreUserIdentifierType.Email,
                Value = resultData.OpenId
            };

            // Validate
            return await ValidateCallbackAsync(identifier, cancellationToken);
        }

        /// <summary>
        /// Validate email registration code
        /// 验证电子邮箱注册验证码
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> ValidateEmailRegistrationAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            var (result, data) = _authCodeService.CreateValidateCodeData(rq, userAgent);
            if (!result.Ok || data == null)
            {
                return result;
            }

            var (resultValidate, resultData) = await _authCodeService.ValidateAsync(AuthCodeAction.UserRegistrationEmailCode, data, cancellationToken);
            if (!resultValidate.Ok || resultData == null)
            {
                return resultValidate;
            }

            // Identifier
            var identifier = new CoreUserIdentifier
            {
                Type = CoreUserIdentifierType.Email,
                Value = resultData.OpenId.ToLower()
            };

            // Validate
            return await ValidateRegistrationAsync(identifier, CoreUserStep.Email, ApplicationErrors.EmailExists.AsResult(), cancellationToken);
        }

        /// <summary>
        /// Validate mobile callback password code
        /// 验证手机找回密码代码
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> ValidateMobileCallbackAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            var (result, data) = _authCodeService.CreateValidateCodeData(rq, userAgent);
            if (!result.Ok || data == null)
            {
                return result;
            }

            var (resultValidate, resultData) = await _authCodeService.ValidateAsync(AuthCodeAction.UserCallbackSMSCode, data, cancellationToken);
            if (!resultValidate.Ok || resultData == null)
            {
                return resultValidate;
            }

            // Identifier
            var identifier = new CoreUserIdentifier
            {
                Type = CoreUserIdentifierType.Mobile,
                Value = resultData.OpenId
            };

            // Validate
            return await ValidateCallbackAsync(identifier, cancellationToken);
        }

        /// <summary>
        /// Validate mobile registration code
        /// 验证手机注册验证码
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> ValidateMobileRegistrationAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            var (result, data) = _authCodeService.CreateValidateCodeData(rq, userAgent);
            if (!result.Ok || data == null)
            {
                return result;
            }

            var (resultValidate, resultData) = await _authCodeService.ValidateAsync(AuthCodeAction.UserRegistrationSMSCode, data, cancellationToken);
            if (!resultValidate.Ok || resultData == null)
            {
                return resultValidate;
            }

            // Identifier
            var identifier = new CoreUserIdentifier
            {
                Type = CoreUserIdentifierType.Mobile,
                Value = resultData.OpenId
            };

            // Validate
            return await ValidateRegistrationAsync(identifier, CoreUserStep.Mobile, ApplicationErrors.MobileExists.AsResult(), cancellationToken);
        }
    }
}