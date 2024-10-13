using com.etsoo.Address;
using com.etsoo.ApiModel.Auth;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.HTTP;
using com.etsoo.Localization;
using com.etsoo.SMS;
using com.etsoo.SMTP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Crypto;
using com.etsoo.Utils.Serialization;
using com.etsoo.Utils.Storage;
using com.etsoo.Web;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Text;
using Npgsql;
using Platform.Server.Application;
using Platform.Server.Database;
using Platform.Server.Database.Models;
using Platform.Server.Dto.Auth;
using Platform.Server.Endpoints.Auth.RQ;
using Platform.Server.Templates;
using System.Globalization;
using System.Net;
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
            public int? AppKeyId { get; init; }
            public required string WebUrl { get; init; }
            public required string ApiUrl { get; init; }
        }

        private const string EncryptDeviceIdKey = "device-id";

        // Code actions
        static List<AuthCodeAction> Actions =>
        [
            new(1, Properties.Resources.UserRegistrationSMSCode, 10, RandStringKind.Digit, 6),
            new(2, Properties.Resources.UserRegistrationEmailCode, 30, RandStringKind.Digit, 6, "/Templates/EmailRegistration.cshtml"),
            new(3, Properties.Resources.UserCallbackSMSCode, 10, RandStringKind.Digit, 6),
            new(4, Properties.Resources.UserCallbackEmailCode, 30, RandStringKind.Digit, 6, "/Templates/EmailCallback.cshtml"),
            new(5, Properties.Resources.UserRegistrationSMSCode, 10, RandStringKind.Digit, 6),
            new(6, Properties.Resources.UserRegistrationEmailCode, 30, RandStringKind.Digit, 6, "/Templates/EmailVerification.cshtml"),
        ];

        // 检查用户登录编号
        /// <summary>
        /// Check user login id
        /// 检查用户登录编号
        /// </summary>
        /// <param name="id">Email or mobile</param>
        /// <param name="region">Region</param>
        /// <param name="isEmail">Is email</param>
        /// <returns>Action result</returns>
        static ActionResult CheckId(ref string id, string region, out bool isEmail)
        {
            isEmail = id.Contains('@');
            if (isEmail)
            {
                // Try parse
                if (MailboxAddress.TryParse(id, out var emailAddress))
                {
                    id = emailAddress.Address;
                }
                else
                {
                    return ApplicationErrors.InvalidEmail.AsResult();
                }
            }
            else
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
        readonly ISMSClient _smsClient;
        readonly ISMTPClient _smtpClient;
        readonly string _root;
        readonly IPAddress _ip;
        readonly IMinUserToken? _regUser;

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
        /// <param name="smsClient">SMS client</param>
        /// <param name="smtpClient">SMTP client</param>
        /// <param name="host">Host environment</param>
        public AuthService(MyDbContext db, IMyApp app, IMyUserAccessor userAccessor, ILogger<AuthService> logger,
            IStorage storage, IHttpClientFactory httpClientFactory,
            ISMSClient smsClient, ISMTPClient smtpClient, IWebHostEnvironment host)
            : base(app, userAccessor.User, "auth", logger)
        {
            _db = db;
            _storage = storage;
            _httpClientFactory=httpClientFactory;

            _smsClient = smsClient;
            _smtpClient = smtpClient;
            _root = host.ContentRootPath;
            _ip = userAccessor.Ip;
            _regUser = userAccessor.User == null ? userAccessor.CreateUserFromAuthorization<MinUserToken>(app.AuthService, Constants.RegistrationTokenAudience, Constants.RegistrationTokenScheme) : null;
        }

        private async Task<AppData?> AuthGetAppSecretAsync(int appId, string appKey, CancellationToken cancellationToken)
        {
            AppData? data;

            if (string.IsNullOrEmpty(appKey))
            {
                data = await _db.CoreApps.AsNoTracking().Where(a => a.Id == appId).Select(a => new AppData { AppSecret = a.AppSecret, WebUrl = a.WebUrl, ApiUrl = a.ApiUrl }).FirstOrDefaultAsync(cancellationToken);
            }
            else
            {
                data = await _db.CoreOrganizationAppKeys.AsNoTracking().Where(ak => ak.CoreOrganizationAppId == appId && ak.AppKey == appKey).Select(a => new AppData { AppSecret = a.AppSecret, AppKeyId = a.Id, WebUrl = a.LocalUrl ?? a.CoreOrganizationApp.CoreApp.WebUrl, ApiUrl = a.LocalApi ?? a.CoreOrganizationApp.CoreApp.ApiUrl }).FirstOrDefaultAsync(cancellationToken);
            }

            if (data != null)
            {
                data.AppSecret = App.DecriptData(data.AppSecret, "Token");
            }

            return data;
        }

        private async Task<AppTokenData> CreateAppTokenDataAsync(CurrentUser user, int appId, int? appKeyId, string appSecret, bool isOffline, TokenQueryData data, CancellationToken cancellationToken)
        {
            var accessToken = App.AuthService.CreateAccessToken(user, null, App.AuthService.AccessTokenMinutes);

            string? refreshToken = null;
            if (isOffline)
            {
                refreshToken = await CreateRefreshTokenAsync(user.IdInt, data.DeviceId, data.Culture, TokenResponseType.Token, data.Data, appId, appKeyId, cancellationToken);
            }

            var token = new AppTokenData
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = App.AuthService.AccessTokenMinutes * 60,
                RefreshToken = refreshToken,
                Scope = string.Join(' ', user.Scopes!),
                IdToken = App.AuthService.CreateIdToken(user.CreateIdentity(), appSecret)
            };

            return token;
        }

        private async Task<string> CreateAuthCodeAsync(CurrentUser user, int appId, int? appKeyId, TokenQueryData data, CancellationToken cancellationToken)
        {
            var code = await CreateRefreshTokenAsync(user.IdInt, data.DeviceId, data.Culture, TokenResponseType.Code, data.Data, appId, appKeyId, cancellationToken);
            return code;
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
            var user = currentUser with { Scopes = scopes };

            var result = ActionResult.Success;

            var redirectUri = rq.RedirectUri.ToString();
            var url = new StringBuilder(redirectUri);
            url.Append('?');

            var appData = await AuthGetAppSecretAsync(rq.AppId, rq.AppKey, cancellationToken);

            if (appData == null)
            {
                result = ApplicationErrors.NoValidData.AsResult("AppId");
            }
            else if (!redirectUri.StartsWith(appData.WebUrl) && !redirectUri.StartsWith(appData.ApiUrl))
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
                            var token = await CreateAppTokenDataAsync(user, rq.AppId, appData.AppKeyId, appData.AppSecret, rq.AccessType == AuthRequest.OfflineAccessType, tokenQueryData, cancellationToken);
                            var tokenJson = JsonSerializer.Serialize(token, ModelJsonSerializerContext.Default.AppTokenData);
                            url.Append($"token={HttpUtility.UrlEncode(tokenJson)}");
                        }
                        else if (rq.ResponseType.Equals(AuthRequest.CodeResponseType))
                        {
                            var code = await CreateAuthCodeAsync(user, rq.AppId, appData.AppKeyId, tokenQueryData, cancellationToken);
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
            user.Region = rq.Region;
            user.Step = 0;

            _db.CoreUsers.Update(user);
            await _db.SaveChangesAsync(cancellationToken);

            return await CompleteLoginAsync(user, rq.DeviceId, deviceName, DeviceType.Web, rq.Region, null, null, rq.Auth, cancellationToken);
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

            var (result, login) = await LoginIdAsync(id, rq.Region, cancellationToken);
            if (!result.Ok || login == null)
            {
                return (result, null);
            }

            // Hash password
            var hashedPassword = await App.HashPasswordAsync(login.Id + password);
            if (string.IsNullOrEmpty(hashedPassword) || !hashedPassword.Equals(login.Password))
            {
                return (ApplicationErrors.NoPasswordMatch.AsResult(), null);
            }

            var user = await _db.CoreUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == login.Id, cancellationToken);
            if (user == null)
            {
                return (ApplicationErrors.NoValidData.AsResult("User"), null);
            }

            var deviceName = cd.Value.Parser.ToShortName();

            return await CompleteLoginAsync(user, rq.DeviceId, deviceName, DeviceType.Web, rq.Region, null, null, rq.Auth, cancellationToken);
        }

        // Complete registration - 完成注册
        // Login with password - 使用密码登录
        // API Refresh token - 接口刷新令牌
        // Log in from OAuth2 client - 从OAuth2客户端登录
        // Sign up from OAuth2 client - 从OAuth2客户端注册
        private async Task<(ActionResult result, string? refreshToken)> CompleteLoginAsync(CoreUser user, string clientId, string deviceName, DeviceType deviceType, string region, int? organizationId, int? fromOrganizationId, AuthRequest? auth, CancellationToken cancellationToken)
        {
            // Default values
            var culture = CultureInfo.CurrentCulture.Name;

            // Token user may pass 0 for organization id
            if (organizationId < 1) organizationId = null;
            if (fromOrganizationId < 1) fromOrganizationId = null;

            // Complete login with the SP
            var userIdSP = new NpgsqlParameter<int>("p_user_id", user.Id);
            var latestOrganizationIdSP = new NpgsqlParameter<int?>("p_latest_organization_id", user.LatestOrganizationId);
            var organizationIdSP = new NpgsqlParameter<int?>("p_target_organization_id", organizationId);
            var fromOrganizationIdSP = new NpgsqlParameter<int?>("p_from_organization_id", fromOrganizationId);
            var deviceNameSP = new NpgsqlParameter<string>("p_device_name", deviceName);
            var deviceTypeSP = new NpgsqlParameter<short>("p_device_type", (byte)deviceType);
            var clientIdSP = new NpgsqlParameter<string>("p_client_id", clientId);
            var ipSP = new NpgsqlParameter<string>("p_ip", _ip.ToString());
            var cultureSP = new NpgsqlParameter<string>("p_culture", culture);

            // IQuerable<T>.FirstOrDefault() adds SQL that filters by the first row number
            // Here we use the stored procedure to get the first row, not the filter
            // CALL SP_NAME
            var data = (await _db.Database.SqlQuery<CompleteLoginData>($"SELECT * FROM complete_login({userIdSP}, {latestOrganizationIdSP}, {organizationIdSP}, {fromOrganizationIdSP}, {deviceNameSP}, {deviceTypeSP}, {clientIdSP}, {ipSP}, {cultureSP})")
                .ToListAsync(cancellationToken)).FirstOrDefault();

            if (data == null)
            {
                return (ApplicationErrors.DataProcessingFailed.AsResult("Data"), null);
            }
            else if (organizationId.HasValue && !organizationId.Equals(data.OrganizationId))
            {
                // Required organization id is invalid
                return (ApplicationErrors.NoValidData.AsResult("OrganizationId"), null);
            }
            else if (fromOrganizationId.HasValue && !fromOrganizationId.Equals(data.ChannelOrganizationId))
            {
                // Required from organization id is invalid
                return (ApplicationErrors.NoValidData.AsResult("FromOrganizationId"), null);
            }

            // Permission scopes
            var scopes = new List<string>
            {
                "core"
            };

            // Is super admin
            // Make sure it's not a partner organization and the user is manager or above
            if (App.Configuration.SuperAdminOrganizationId.Equals(data.OrganizationId)
                && data.ParentOrganizationId == null && data.ChannelOrganizationId == null
                && data.UserRole >= UserRole.Manager)
            {
                scopes.Add("super");
            }

            // App paid scopes
            scopes.AddRange(data.Scopes.Select(s => $"app{s}"));

            // Limit scopes with auth request
            if (auth != null)
            {
                scopes = scopes.Intersect(auth.Scopes).ToList();
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
                Name = user.Name,
                Avatar = user.Avatar,
                OrganizationName = data.OrganizationName,
                Oid = data.Oid,
                Role = data.UserRole
            });

            if (tokenUser == null)
            {
                return (result, null);
            }

            if (auth == null)
            {
                var minutes = App.AuthService.AccessTokenMinutes;
                var accessToken = App.AuthService.CreateAccessToken(tokenUser, null, minutes);

                // Refresh token
                var refreshToken = await CreateRefreshTokenAsync(user.Id, data.DeviceId, culture, TokenResponseType.Token, tokenData, null, null, cancellationToken);

                // Serverside device id
                // Encrypt DeviceId for client identifier
                var deviceId = await HashEncryptAsync(data.DeviceId.ToString(), App.Configuration.InitCallEncryptionIdentifier);

                var publicData = new PublicUserData
                {
                    Name = user.Name,
                    Avatar = user.Avatar,
                    Organization = data.OrganizationId,
                    IsChannel = data.ChannelOrganizationId.HasValue,
                    IsParent = data.ParentOrganizationId.HasValue,
                    Role = (short)data.UserRole,
                    TokenScheme = "Bearer",
                    Token = accessToken,
                    Seconds = 60 * minutes,
                    DeviceId = deviceId
                };

                // Save
                publicData.SaveTo(result);

                return (result, refreshToken);
            }
            else
            {
                var uri = await AuthRequestAsync(auth, tokenUser, tokenQueryData, cancellationToken);
                return (result, uri);
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
        /// <param name="id">Email or mobile</param>
        /// <param name="region">Country or region id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple results</returns>
        public async ValueTask<(IActionResult, LoginUserWithPassword?)> LoginIdAsync(string id, string region, CancellationToken cancellationToken = default)
        {
            // Check
            var result = CheckId(ref id, region, out var isEmail);
            if (!result.Ok)
            {
                return (result, null);
            }

            var type = isEmail ? CoreUserIdentifierType.Email : CoreUserIdentifierType.Mobile;

            // Login with id check
            var data = await _db.CoreUserIdentifiers
                .AsNoTracking()
                .Where(i => i.Type == type && i.Value == id)
                .Select(i => new LoginUserWithPassword { Id = i.CoreUser.Id, Password = i.CoreUser.Password, Status = i.CoreUser.Status, FrozenTime = i.CoreUser.FrozenTime, Step = i.CoreUser.Step })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return (ApplicationErrors.NoUserFound.AsResult(), null);
            }

            result = ValidateUser(data);
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

        private string CreateRegistrationToken(int id)
        {
            var user = new MinUserToken
            {
                Id = id.ToString(),
                Scopes = ["core"]
            };

            return App.AuthService.CreateAccessToken(user, Constants.RegistrationTokenAudience, 60);
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

        private async Task<string> CreateRefreshTokenAsync(int userId, int deviceId, string culture, TokenResponseType responseType, DeviceTokenData data, int? appId, int? appKeyId, CancellationToken cancellationToken)
        {
            // Use the latest token to avoid huge creation of new one
            var tokenData = await _db.CoreUserDeviceTokens
                .AsNoTracking()
                .Where(t => t.DeviceId == deviceId && t.ResponseType == responseType && t.AppId == appId && t.AppKeyId == appKeyId)
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
                    AppKeyId = appKeyId,
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
                    t.Device.CoreUser,
                    t.Device.DeviceType,
                    Data = new TokenQueryData
                    {
                        DeviceId = t.DeviceId,
                        UserId = t.Device.CoreUser.Id,
                        Culture = t.Culture,
                        Data = t.Data
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

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

            return await CompleteLoginAsync(user, data.DeviceId, deviceName, tokenData.DeviceType, td.Region, td.OrganizationId, td.ChannelOrganizationId ?? td.ParentOrganizationId, null, cancellationToken);
        }

        private ActionResult ValidateUser(LoginUser user)
        {
            if (user.FrozenTime.HasValue)
            {
                var result = ApplicationErrors.UserFrozen.AsResult();
                if (result.Title != null)
                    result.Title = string.Format(result.Title, user.FrozenTime.ToString());
                return result;
            }
            else if (user.Status > EntityStatus.Approved)
            {
                return ApplicationErrors.AccountDisabled.AsResult("Status");
            }
            else if (user.OrgStatus != null && user.OrgStatus > EntityStatus.Approved)
            {
                return ApplicationErrors.AccountDisabled.AsResult("OrgStatus");
            }
            else if (user.OrgExpiry != null && user.OrgExpiry < DateTime.UtcNow)
            {
                return ApplicationErrors.AccountDisabled.AsResult("OrgExpiry");
            }
            else
            {
                return ActionResult.Success;
            }
        }

        private async Task ValidateUserAsync(HttpContext context, CoreUserIdentifierType type, LoginUser user, AuthLoginValidateData loginData, CancellationToken cancellationToken)
        {
            var result = ValidateUser(user);
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
            var user = await _db.CoreUsers.FirstOrDefaultAsync(u => u.Id == loginUser.Id, cancellationToken);
            if (user == null)
            {
                RedirectToFailureUrl(context.Response, type, "No User Found");
                return;
            }

            var deviceName = loginData.Parser.ToShortName();

            // No authorization request, will be done in the client side
            var (result, refreshToken) = await CompleteLoginAsync(user, loginData.DeviceId, deviceName, DeviceType.Web, loginData.Region, null, null, null, cancellationToken);

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
                            result = ValidateUser(loginUser);
                            if (result.Ok)
                            {
                                // Current user
                                var currentUser = await _db.CoreUsers.FindAsync(loginUser.Id, cancellationToken);
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

        private string HashAuthCode(short id, string code, DateTime expiry)
        {
            return App.HashPassword($"{id}-{code}-{expiry.ToBinary()}");
        }

        private CoreAuthCode CreateAuthCode(AuthCodeAction action, string openid, out string code)
        {
            // Code
            code = CryptographyUtils.CreateRandString(action.Kind, action.Length).ToString();

            // Expiry
            // Miliseconds with same accuracy with Database rounded to increments of .000, .003, or .007 seconds
            var expiry = DateTime.UtcNow.AddMinutes(action.Minutes).ToSqlDateTime();

            // Code hashed
            var codeHashed = HashAuthCode(action.Id, code, expiry);

            return new CoreAuthCode
            {
                Id = Guid.NewGuid(),
                Action = action.Id,
                Openid = openid,
                Code = codeHashed,
                Expiry = expiry,
                Ip= _ip
            };
        }

        private async Task<IActionResult> AddAuthCodeAsync(CoreAuthCode authCode, CancellationToken cancellationToken)
        {
            // Save
            _db.CoreAuthCodes.Add(authCode);
            if (await _db.SaveChangesAsync(cancellationToken) > 0)
            {
                // Hold the id
                var result = ActionResult.Success;
                result.Data.Add("Id", authCode.Id);
                return result;
            }
            else
            {
                return ApplicationErrors.DataProcessingFailed.AsResult();
            }
        }

        private async Task<IActionResult> CheckFrequencyAsync(AuthCodeAction action, string openid, CancellationToken cancellationToken)
        {
            // Check frequency
            var lastMinutes = DateTime.UtcNow.AddMinutes(-2);
            var lastExists = await _db.CoreAuthCodes
                .AsNoTracking()
                .AnyAsync(c => c.Action == action.Id && c.Openid == openid && c.Creation > lastMinutes, cancellationToken);

            if (lastExists)
            {
                return ApplicationErrors.HighRequestRrequency.AsResult();
            }

            var lasthours = DateTime.UtcNow.AddHours(-1);
            var lastHoursCount = await _db.CoreAuthCodes
                .AsNoTracking()
                .CountAsync(c => c.Action == action.Id && c.Openid == openid && c.Creation > lasthours, cancellationToken);

            if (lastHoursCount > 10)
            {
                return ApplicationErrors.HighRequestRrequency.AsResult("Creation");
            }

            var lastIpCount = await _db.CoreAuthCodes
                .AsNoTracking()
                .CountAsync(c => c.Ip.Equals(_ip) && c.Creation > lasthours, cancellationToken);

            if (lastIpCount > 1000)
            {
                return ApplicationErrors.HighRequestRrequency.AsResult("IP");
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// Send Email code
        /// 发送邮件验证码
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> SendEmailAsync(SendEmailData data, CancellationToken cancellationToken = default)
        {
            var email = data.Email;
            if (!MailboxAddress.TryParse(email, out var emailAddress))
            {
                return ApplicationErrors.InvalidEmail.AsResult();
            }

            // Action
            var action = Actions.Find(a => a.Id == data.Action);
            if (action == null || action.Template == null)
            {
                return ApplicationErrors.NoValidData.AsResult("Action");
            }

            // Check frequency
            var result = await CheckFrequencyAsync(action, email, cancellationToken);
            if (!result.Ok)
            {
                return result;
            }

            // Auth code
            var authCode = CreateAuthCode(action, email, out var code);

            try
            {
                // Time zone
                var tz = LocalizationUtils.GetTimeZone(data.TimeZone);

                // Model
                var dataModel = new AuthCodeEmailTemplateView
                {
                    Action = action,
                    Code = code,
                    Language = CultureInfo.CurrentCulture.Name,
                    TimeZone = tz,
                    LocalExpiry = authCode.Expiry.UtcToLocal(tz)
                };

                // Template
                var file = Path.Join(_root, action.Template);
                var template = await RazorUtils.RenderAsync(file, dataModel);

                // Message
                var message = new MimeMessage
                {
                    Subject = dataModel.Subject,
                    Body = new TextPart(TextFormat.Html) { Text = template }
                };
                message.To.Add(emailAddress);

                // Send
                await _smtpClient.SendAsync(message, cancellationToken);

                // Save
                result = await AddAuthCodeAsync(authCode, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log for trace
                ex.Data.Add("Model", JsonSerializer.Serialize(data, MyJsonSerializerContext.Default.SendSMSData));
                ex.Data.Add("Action", JsonSerializer.Serialize(action, MyJsonSerializerContext.Default.AuthCodeAction));

                LogException(ex);

                // New result
                result = ApplicationErrors.CodeSendingFailed.AsResult();
            }

            return result;
        }

        /// <summary>
        /// Send SMS code
        /// 发送短信验证码
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> SendSMSAsync(SendSMSData data, CancellationToken cancellationToken = default)
        {
            // Mobile
            var mobile = AddressRegion.CreatePhone(data.Mobile, data.Region ?? _smsClient.Region.Id);
            if (mobile == null)
            {
                return ApplicationErrors.InvalidMobile.AsResult("Mobile");
            }

            // Action
            var action = Actions.Find(a => a.Id == data.Action);
            if (action == null)
            {
                return ApplicationErrors.NoValidData.AsResult("Action");
            }

            var mobileString = mobile.ToInternationalFormat();

            // Check frequency
            var result = await CheckFrequencyAsync(action, mobileString, cancellationToken);
            if (!result.Ok)
            {
                return result;
            }

            // Auth code
            var authCode = CreateAuthCode(action, mobileString, out var code);

            // Template
            var template = _smsClient.GetTemplate(TemplateKind.Code, region: data.Region, language: CultureInfo.CurrentCulture.Name);
            if (template == null)
            {
                return ApplicationErrors.NoValidData.AsResult("Template");
            }

            // Send
            var smsResult = await _smsClient.SendCodeAsync(mobile, code, template, cancellationToken);

            if (smsResult.Ok)
            {
                // Save
                result = await AddAuthCodeAsync(authCode, cancellationToken);
            }
            else
            {
                // Log for trace
                var exception = new Exception(smsResult.Title);
                exception.Data.Add("Model", JsonSerializer.Serialize(data));
                exception.Data.Add("Template", JsonSerializer.Serialize(template));
                exception.Data.Add("SMSResult", JsonSerializer.Serialize(smsResult));

                LogException(exception);

                // New result
                result = ApplicationErrors.CodeSendingFailed.AsResult();
            }

            // Return
            return result;
        }

        /// <summary>
        /// Validate code
        /// 验证验证码
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        private async Task<(ActionResult result, string? openId, int? userId)> ValidateAsync(short actionId, ValidateCodeData data, CancellationToken cancellationToken = default)
        {
            // Auth code
            var code = await _db.CoreAuthCodes.FirstOrDefaultAsync(c => c.Id == data.Id && c.Action == actionId, cancellationToken);
            if (code == null || code.Expiry.Subtract(DateTime.UtcNow).TotalMilliseconds < 0)
            {
                // Deleted or expired
                return (ApplicationErrors.CodeExpired.AsResult(), null, null);
            }

            if (!code.Ip.Equals(_ip))
            {
                // IP address changed
                return (ApplicationErrors.IPAddressChanged.AsResult(), null, null);
            }

            var codeHashed = HashAuthCode(actionId, data.Code, code.Expiry);
            if (code.Code != codeHashed)
            {
                // No match
                if (code.Times > 5)
                {
                    // Ignore instead of delete
                    // _db.CoreAuthCodes.Remove(code);
                }
                else
                {
                    // Update
                    code.Times++;
                    _db.CoreAuthCodes.Update(code);
                    await _db.SaveChangesAsync(cancellationToken);
                }
                return (ApplicationErrors.CodesNoMatch.AsResult(), null, null);
            }

            // Return data
            var openId = code.Openid;
            var userId = code.CoreUserId;

            // Delete
            _db.CoreAuthCodes.Remove(code);
            await _db.SaveChangesAsync(cancellationToken);

            return (ActionResult.Success, openId, userId);
        }

        /// <summary>
        /// Validate registration
        /// 验证注册
        /// </summary>
        /// <returns>Task</returns>
        public async Task<ActionResult> ValidateRegistrationAsync(CoreUserIdentifier identifier, short step, ActionResult existError, CancellationToken cancellationToken = default)
        {
            int userId;

            if (_regUser == null)
            {
                // Find the user
                var user = await _db.CoreUsers.FirstOrDefaultAsync(u => u.CoreUserIdentifiers.Any(i => i.Type == identifier.Type && i.Value == identifier.Value), cancellationToken);

                if (user == null)
                {
                    // New user
                    user = new CoreUser
                    {
                        Name = string.Empty,
                        Step = step
                    };

                    user.CoreUserIdentifiers.Add(identifier);

                    // AddAsync vs Add
                    await _db.CoreUsers.AddAsync(user, cancellationToken);
                    await _db.SaveChangesAsync(cancellationToken);

                    userId = user.Id;
                }
                else if (user.Step == 0)
                {
                    // Registered
                    // Not secure to return the user with one time code
                    return existError;
                }
                else
                {
                    // Continue to register
                    userId = user.Id;
                }
            }
            else
            {
                // Update the user
                var user = await _db.CoreUsers.Include(u => u.CoreUserIdentifiers.Where(i => i.Type == identifier.Type)).FirstOrDefaultAsync(u => u.Id == _regUser.IdInt, cancellationToken);
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
        /// Validate email registration code
        /// 验证电子邮箱注册验证码
        /// </summary>
        /// <returns>Task</returns>
        public async Task<ActionResult> ValidateEmailRegistrationAsync(ValidateCodeData data, CancellationToken cancellationToken = default)
        {
            var (result, email, _) = await ValidateAsync(2, data, cancellationToken);
            if (!result.Ok || email == null)
            {
                return result;
            }

            // Identifier
            var identifier = new CoreUserIdentifier
            {
                Type = CoreUserIdentifierType.Email,
                Value = email
            };

            // Validate
            return await ValidateRegistrationAsync(identifier, CoreUserStep.Email, ApplicationErrors.EmailExists.AsResult(), cancellationToken);
        }

        /// <summary>
        /// Validate mobile registration code
        /// 验证手机注册验证码
        /// </summary>
        /// <returns>Task</returns>
        public async Task<ActionResult> ValidateMobileRegistrationAsync(ValidateCodeData data, CancellationToken cancellationToken = default)
        {
            var (result, mobile, _) = await ValidateAsync(1, data, cancellationToken);
            if (!result.Ok || mobile == null)
            {
                return result;
            }

            // Identifier
            var identifier = new CoreUserIdentifier
            {
                Type = CoreUserIdentifierType.Mobile,
                Value = mobile
            };

            // Validate
            return await ValidateRegistrationAsync(identifier, CoreUserStep.Mobile, ApplicationErrors.MobileExists.AsResult(), cancellationToken);
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
            var (result, user) = await CreateUserFromQueryDataAsync(data, cancellationToken);
            if (user == null)
            {
                Logger.LogWarning("User not found with @{data}, @{result}", data, result);
                return null;
            }

            return await CreateAppTokenDataAsync(user, rq.AppId, appData.AppKeyId, appData.AppSecret, data.Data.AccessType == AuthRequest.OfflineAccessType, data, cancellationToken);
        }

        private async Task<(ActionResult, CurrentUser?)> CreateUserFromQueryDataAsync(TokenQueryData data, CancellationToken cancellationToken)
        {
            var userData = await _db.CoreUsers
                .AsNoTracking()
                .Where(u => u.Id == data.UserId)
                .GroupJoin(_db.CoreOrganizationUsers, u => u.Id, ou => ou.CoreUserId, (u, ou) => new { u, ou })
                .SelectMany(d => d.ou.Where(ou => ou.CoreOrganizationId == data.Data.OrganizationId).DefaultIfEmpty(), (d, ou) => new TokenQueryUser
                {
                    Id = d.u.Id,
                    Status = d.u.Status,
                    FrozenTime = d.u.FrozenTime,
                    Step = d.u.Step,
                    OrgStatus = ou == null ? null : ou.Status,
                    OrgExpiry = ou == null ? null : ou.Expiry,
                    Name = ou == null ? d.u.Name : (ou.LocalName ?? d.u.Name),
                    Avatar = ou == null ? d.u.Avatar : (ou.LocalAvatar ?? d.u.Avatar),
                    Role = ou == null ? null : ou.UserRole,
                    Oid = ou == null ? null : ou.Id
                })
                .FirstOrDefaultAsync(cancellationToken);

            return CreateUserFrom(data, userData);
        }

        private (ActionResult, CurrentUser?) CreateUserFrom(TokenQueryData data, TokenQueryUser? userData)
        {
            if (userData == null)
            {
                return (ApplicationErrors.NoUserFound.AsResult(), null);
            }

            var result = ValidateUser(userData);
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
                Avatar = userData.Avatar,
                DeviceId = data.DeviceId.ToString(),
                Organization = data.Data.OrganizationId.ToString(),
                OrganizationName = userData.OrganizationName,
                ParentOrganization = data.Data.ParentOrganizationId?.ToString(),
                ChannelOrganization = data.Data.ChannelOrganizationId?.ToString(),
                Oid = userData.Oid.GetValueOrDefault().ToString(),
                Uid = data.Data.Uid.ToString(),
                RoleValue = (short)userData.Role.GetValueOrDefault(UserRole.User),
                Scopes = data.Data.Scopes,
                ClientIp = _ip,
                Language = new CultureInfo(data.Culture),
                Region = data.Data.Region
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

            var tokenData = await _db.CoreUserDeviceTokens
                .AsNoTracking()
                .Where(d => d.ResponseType == TokenResponseType.Token && d.AppId == rq.AppId && d.Token == rq.RefreshToken)
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
            var (result, user) = await CreateUserFromQueryDataAsync(data, cancellationToken);
            if (user == null || !result.Ok)
            {
                Logger.LogWarning("User not found with @{data}, @{result}", data, result);
                return null;
            }

            return await CreateAppTokenDataAsync(user, rq.AppId, appData.AppKeyId, appData.AppSecret, true, data, cancellationToken);
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
                    d.Device.CoreUser,
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

            return await CompleteLoginAsync(tokenData.CoreUser, tokenData.ClientId, tokenData.Name, tokenData.DeviceType, td.Region, td.OrganizationId, td.ChannelOrganizationId ?? td.ParentOrganizationId, null, cancellationToken);
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
        public async ValueTask<(IActionResult result, string? refreshToken)> SwitchOrgAsync(SwitchOrgRQ rq, CancellationToken cancellationToken = default)
        {
            if (User == null)
            {
                return (ApplicationErrors.AccessDenied.AsResult(), null);
            }

            var data = await _db.CoreUserDevices.AsNoTracking().Where(d => d.Id == User.DeviceIdInt && d.CoreUserId == User.IdInt)
                .Select(d => new { d.Name, d.DeviceType, d.ClientId, d.CoreUser })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return (ApplicationErrors.NoUserFound.AsResult(), null);
            }

            return await CompleteLoginAsync(data.CoreUser, data.ClientId, data.Name, data.DeviceType, User.Region, rq.OrganizationId, rq.FromOrganizationId, null, cancellationToken);
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
    }
}