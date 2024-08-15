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
using com.etsoo.Utils.Storage;
using com.etsoo.Web;
using com.etsoo.WebUtils;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Text;
using Npgsql;
using Platform.Server.Application;
using Platform.Server.Database;
using Platform.Server.Database.Models;
using Platform.Server.Dto.Auth;
using Platform.Server.Templates;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Web;

namespace Platform.Server.Services
{
    public class AuthService : CommonService, IAuthService
    {
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
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        /// <param name="httpClientFactory">HTTP client factory</param>
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
            _regUser = userAccessor.User == null && app.AuthService != null ? userAccessor.CreateUserFromAuthorization<MinUserToken>(app.AuthService, Constants.RegistrationTokenAudience, Constants.RegistrationTokenScheme) : null;
        }

        public async ValueTask<IActionResult> CompleteRegisterAsync(CompleteRegisterData data, CancellationToken cancellationToken = default)
        {
            if (_regUser == null)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Hash password
            var password = await App.HashPasswordAsync(_regUser.Id + data.Password);

            // Update the user
            var user = await _db.CoreUsers.FirstOrDefaultAsync(u => u.Id == _regUser.IdInt, cancellationToken);
            if (user == null)
            {
                return ApplicationErrors.NoValidData.AsResult("User");
            }

            user.Password = password;
            user.Name = data.Name;
            user.Region = data.Region;
            user.Step = 0;

            _db.CoreUsers.Update(user);
            await _db.SaveChangesAsync(cancellationToken);

            return await CompleteLoginAsync(user, data.DeviceName, DeviceType.Web, data.Region, null, null, cancellationToken);
        }

        public async ValueTask<IActionResult> LoginWithPwdAsync(LoginData data, CancellationToken cancellationToken = default)
        {
            var (result, login) = await LoginIdAsync(data.Id, data.Region, cancellationToken);
            if (!result.Ok || login == null)
            {
                return result;
            }

            // Hash password
            var password = await App.HashPasswordAsync(login.Id + data.Password);
            if (string.IsNullOrEmpty(password) || !password.Equals(login.Password))
            {
                return ApplicationErrors.NoPasswordMatch.AsResult();
            }

            var user = await _db.CoreUsers.FirstOrDefaultAsync(u => u.Id == login.Id, cancellationToken);
            if (user == null)
            {
                return ApplicationErrors.NoValidData.AsResult("User");
            }

            return await CompleteLoginAsync(user, data.DeviceName, DeviceType.Web, data.Region, null, null, cancellationToken);
        }

        private async Task<ActionResult> CompleteLoginAsync(CoreUser user, string deviceName, DeviceType deviceType, string region, int? organizationId, int? fromOrganizationId, CancellationToken cancellationToken)
        {
            if (App.AuthService == null)
            {
                throw new Exception("No Authorization Service");
            }

            var culture = CultureInfo.CurrentCulture.Name;

            // Complete login with the SP
            var userIdSP = new NpgsqlParameter<int>("user_id", user.Id);
            var latestOrganizationIdSP = new NpgsqlParameter<int?>("latest_organization_id", user.LatestOrganizationId);
            var organizationIdSP = new NpgsqlParameter<int?>("organization_id", organizationId);
            var fromOrganizationIdSP = new NpgsqlParameter<int?>("from_organization_id", fromOrganizationId);
            var deviceNameSP = new NpgsqlParameter<string>("device_name", deviceName);
            var deviceTypeSP = new NpgsqlParameter<short>("device_type", (byte)deviceType);
            var ipSP = new NpgsqlParameter<string>("ip", _ip.ToString());
            var cultureSP = new NpgsqlParameter<string>("culture", culture);

            // IQuerable<T>.FirstOrDefault() adds SQL that filters by the first row number
            // Here we use the stored procedure to get the first row, not the filter
            // CALL SP_NAME
            var data = (await _db.Database.SqlQuery<CompleteLoginData>($"SELECT * FROM complete_login({userIdSP}, {latestOrganizationIdSP}, {organizationIdSP}, {fromOrganizationIdSP}, {deviceNameSP}, {deviceTypeSP}, {ipSP}, {cultureSP})")
                .ToListAsync(cancellationToken)).FirstOrDefault();

            if (data == null)
            {
                return ApplicationErrors.DataProcessingFailed.AsResult("Data");
            }
            else if (organizationId.HasValue && !organizationId.Equals(data.TestOrganizationId))
            {
                // Required organization id is invalid
                return ApplicationErrors.NoValidData.AsResult("OrganizationId");
            }
            else if (fromOrganizationId.HasValue && !fromOrganizationId.Equals(data.ChannelOrganizationId))
            {
                // Required from organization id is invalid
                return ApplicationErrors.NoValidData.AsResult("FromOrganizationId");
            }

            // Permission scopes
            var scopes = new List<string>
            {
                "core"
            };

            // Is super admin
            // Make sure it's not a partner organization and the user is manager or above
            if (App.Configuration.SuperAdminOrganizationId.Equals(data.TestOrganizationId)
                && data.ParentOrganizationId == null && data.ChannelOrganizationId == null
                && data.UserRole >= UserRole.Manager)
            {
                scopes.Add("super");
            }

            // App scopes
            scopes.AddRange(data.Scopes.Select(s => $"app{s}"));

            var role = (short)data.UserRole;

            var tokenUser = new CurrentUser(
                user.Id.ToString(),
                scopes,
                data.TestOrganizationId.GetValueOrDefault().ToString(),
                user.Name,
                role,
                _ip,
                data.DeviceId.ToString(),
                CultureInfo.CurrentCulture,
                region,
                data.Oid.GetValueOrDefault().ToString(),
                user.Avatar,
                data.OrganizationName,
                data.ChannelOrganizationId.ToString(),
                data.ParentOrganizationId.ToString(),
                null
            );

            var minutes = App.AuthService.AccessTokenMinutes;
            var accessToken = App.AuthService.CreateAccessToken(tokenUser, null, minutes);

            // Result
            var result = ActionResult.Success;

            result.Data["Name"] = user.Name;
            result.Data["Avatar"] = user.Avatar;
            result.Data["Organization"] = data.TestOrganizationId;
            result.Data["Role"] = role;
            result.Data["Token"] = accessToken;
            result.Data["Seconds"] = 60 * minutes;
            result.Data["Uid"] = user.Id.ToString();

            return result;
        }

        /// <summary>
        /// Get log in URL
        /// 获取登录URL
        /// </summary>
        /// <param name="client">Auth client</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="deviceId">Device id</param>
        /// <returns>Result</returns>
        public IResult GetLogInUrl(IAuthClient client, string? userAgent, string deviceId)
        {
            if (!this.CheckDevice(userAgent, deviceId, out var result, out _))
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
        /// <param name="deviceId">Device id</param>
        /// <returns>Result</returns>
        public IResult GetSignUpUrl(IAuthClient client, string? userAgent, string deviceId)
        {
            if (!this.CheckDevice(userAgent, deviceId, out var result, out _))
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
            var (result, userInfo) = await client.GetUserInfoAsync(context.Request, (s) => this.CheckDevice(context.UserAgent(), s.Replace(" ", "+")), null, cancellationToken);

            if (result.Ok && userInfo != null)
            {
                var loginUser = await ReadUserAsync(type, userInfo.OpenId);
                if (loginUser == null)
                {
                    var url = $"{App.Configuration.AuthFailureUrl}?type={type}";
                    context.Response.Redirect(url);
                }
                else
                {
                    ValidateUser(context.Response, type, loginUser);
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

            // Login with id check
            var data = await _db.CoreUserIdentifiers
                .Where(i => i.Type == (isEmail ? CoreUserIdentifierType.Email : CoreUserIdentifierType.Mobile) && i.Value == id)
                .Select(i => new LoginUserWithPassword { Id = i.CoreUser.Id, Password = i.CoreUser.Password, Status = i.CoreUser.Status, FrozenTime = i.CoreUser.FrozenTime, Step = i.CoreUser.Step })
                .FirstOrDefaultAsync();

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
            return await _db.CoreUserIdentifiers.Where(i => i.Type == type && i.Value == openId)
                .Select(i => new LoginUser { Id = i.CoreUser.Id, Status = i.CoreUser.Status, FrozenTime = i.CoreUser.FrozenTime, Step = i.CoreUser.Step })
                .FirstOrDefaultAsync();
        }

        private void RedirectToFailureUrl(HttpResponse response, CoreUserIdentifierType type, string error, string? errorType = null, string? errorField = null)
        {
            var url = $"{App.Configuration.AuthFailureUrl}?type={type}&error={HttpUtility.UrlEncode(error)}&errorType={HttpUtility.UrlEncode(errorType)}&errorField={HttpUtility.UrlEncode(errorField)}";
            response.Redirect(url);
        }

        private string CreateLoginToken(int id)
        {
            if (App.AuthService == null)
            {
                throw new Exception("No Authorization Service");
            }

            var user = new MinUserToken(id.ToString(), ["core"]);
            return App.AuthService.CreateAccessToken(user, Constants.RegistrationTokenAudience, 60);
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
            else
            {
                return ActionResult.Success;
            }
        }

        private void ValidateUser(HttpResponse response, CoreUserIdentifierType type, LoginUser user)
        {
            var result = ValidateUser(user);
            if (result.Ok)
            {
                if (user.Step > 0)
                {
                    var token = CreateLoginToken(user.Id);
                    var url = $"{App.Configuration.AuthRegistrationUrl}{user.Step}?token={HttpUtility.UrlEncode(token)}";
                    response.Redirect(url);
                }
                else
                {
                    response.Redirect(App.Configuration.AuthSuccessUrl);
                }
            }
            else
            {
                RedirectToFailureUrl(response, type, result.Title ?? "Validate User Error", result.Type);
            }
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
            var (result, userInfo) = await client.GetUserInfoAsync(context.Request, (s) => this.CheckDevice(context.UserAgent(), s.Replace(" ", "+")), null, cancellationToken);

            if (result.Ok && userInfo != null)
            {
                var loginUser = await ReadUserAsync(type, userInfo.OpenId);
                if (loginUser == null)
                {
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

                    // Has email
                    var hasEmail = false;
                    if (!string.IsNullOrEmpty(userInfo.Email) && userInfo.EmailVerified is true)
                    {
                        // Check email exists
                        var emailExists = await _db.CoreUserIdentifiers.AnyAsync(i => i.Type == CoreUserIdentifierType.Email && i.Value == userInfo.Email, cancellationToken);

                        if (emailExists)
                        {
                            result = ApplicationErrors.EmailExists.AsResult();
                            RedirectToFailureUrl(context.Response, type, result.Title ?? "Account Email Exists", result.Type, result.Field);
                            return;
                        }

                        hasEmail = true;
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

                    ValidateUser(context.Response, type, new LoginUser { Id = user.Id, Status = user.Status, FrozenTime = user.FrozenTime, Step = user.Step });
                }
                else
                {
                    ValidateUser(context.Response, type, loginUser);
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
        protected override async Task InitCallUpdateAsync(string prevDeviceId, string newDeviceId, int deviceId)
        {
            await Task.CompletedTask;
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
                .AnyAsync(c => c.Action == action.Id && c.Openid == openid && c.Creation > lastMinutes, cancellationToken);

            if (lastExists)
            {
                return ApplicationErrors.HighRequestRrequency.AsResult();
            }

            var lasthours = DateTime.UtcNow.AddHours(-1);
            var lastHoursCount = await _db.CoreAuthCodes
                .CountAsync(c => c.Action == action.Id && c.Openid == openid && c.Creation > lasthours, cancellationToken);

            if (lastHoursCount > 10)
            {
                return ApplicationErrors.HighRequestRrequency.AsResult("Creation");
            }

            var lastIpCount = await _db.CoreAuthCodes
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
                result.Data["token"] = CreateLoginToken(userId);
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
                .Where(u => u.Id == _regUser.IdInt)
                .Select(u => new RegisterUserData { Name = u.Name })
                .FirstOrDefaultAsync(cancellationToken);

            return user;
        }
    }
}