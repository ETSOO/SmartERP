using com.etsoo.Address;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Crypto;
using com.etsoo.Web;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Dto.AuthCode;
using Platform.Server.Endpoints.AuthCode.RQ;
using Platform.Server.Templates;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Platform.Server.Services
{
    /// <summary>
    /// Auth code service
    /// 验证码服务
    /// </summary>
    public class AuthCodeService : CommonService, IAuthCodeService
    {
        // Code actions
        static List<AuthCodeActionItem> Actions =>
        [
            new(AuthCodeAction.UserRegistrationSMSCode, Properties.Resources.UserRegistrationSMSCode, 10, RandStringKind.Digit, 6),
            new(AuthCodeAction.UserRegistrationEmailCode, Properties.Resources.UserRegistrationEmailCode, 30, RandStringKind.Digit, 6, false, "/Templates/EmailRegistration.cshtml"),
            new(AuthCodeAction.UserCallbackSMSCode, Properties.Resources.UserCallbackSMSCode, 10, RandStringKind.Digit, 6),
            new(AuthCodeAction.UserCallbackEmailCode, Properties.Resources.UserCallbackEmailCode, 30, RandStringKind.Digit, 6, false, "/Templates/EmailCallback.cshtml"),
            new(AuthCodeAction.UserVerificationSMSCode, Properties.Resources.UserVerificationSMSCode, 10, RandStringKind.Digit, 6, true),
            new(AuthCodeAction.UserVerificationEmailCode, Properties.Resources.UserVerificationEmailCode, 30, RandStringKind.Digit, 6, true, "/Templates/EmailVerification.cshtml"),

            // Member invitation, 3 days = 72 hours = 4320 minutes
            new(AuthCodeAction.MemberInvitationEmailCode, Properties.Resources.MemberInvitationEmailCode, 4320, RandStringKind.DigitAndLetter, 16, true, "/Templates/EmailMemberInvitation.cshtml")
        ];

        readonly MyDbContext _db;
        readonly string _root;
        readonly IPAddress _ip;
        readonly IQueueService _queueService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="smsClient">SMS client</param>
        /// <param name="host">Host environment</param>
        /// <param name="queueService">Queue service</param>
        public AuthCodeService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<AuthCodeService> logger,
            IWebHostEnvironment host, IQueueService queueService)
            : base(app, userAccessor.User, "auth_code", logger)
        {
            _db = db;
            _root = host.ContentRootPath;
            _ip = userAccessor.Ip;
            _queueService = queueService;
        }

        /// <summary>
        /// Create validate code data
        /// 创建验证码数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <returns>Result</returns>
        public (IActionResult result, ValidateCodeData? data) CreateValidateCodeData(CodeValidateRQ rq, string? userAgent)
        {
            // Check device
            if (!this.CheckDevice(userAgent, rq.DeviceId, out var checkResult, out var cd))
            {
                return (checkResult, null);
            }

            var deviceCore = cd.Value.DeviceCore;

            var code = DecryptDeviceData(rq.Code, deviceCore);
            if (code == null)
            {
                return (ApplicationErrors.NoValidData.AsResult("Code"), null);
            }

            return (ActionResult.Success, new ValidateCodeData
            {
                Code = code,
                Id = rq.Id
            });
        }

        /// <summary>
        /// Send email code
        /// 发送邮件验证码
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> SendEmailAsync(EmailCodeRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            // Check device
            if (!this.CheckDevice(userAgent, rq.DeviceId, out var checkResult, out var cd))
            {
                return checkResult;
            }

            var deviceCore = cd.Value.DeviceCore;

            var email = DecryptDeviceData(rq.Email, deviceCore);
            if (email == null)
            {
                return ApplicationErrors.NoValidData.AsResult("Email");
            }

            var data = new SendEmailData
            {
                Action = rq.Action,
                Email = email,
                Region = rq.Region,
                TimeZone = rq.TimeZone
            };

            return await SendEmailAsync(data, null, cancellationToken);
        }

        /// <summary>
        /// Send Email code
        /// 发送邮件验证码
        /// </summary>
        /// <typeparam name="D">Generic json data type</typeparam>
        /// <param name="data">Data</param>
        /// <param name="typeInfo">JSON type info</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> SendEmailAsync<D>(SendEmailData<D> data, JsonTypeInfo<D> typeInfo, CancellationToken cancellationToken = default) where D : AuthCodeData
        {
            var json = JsonSerializer.Serialize(data.Data, typeInfo);
            return await SendEmailAsync(data,
                (view) => (new AuthCodeEmailTemplateView<D>(view, data.Data), JsonSerializer.Serialize(data.Data, typeInfo)),
                cancellationToken);
        }

        /// <summary>
        /// Send Email code
        /// 发送邮件验证码
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="enhancer">Data enhancer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> SendEmailAsync(SendEmailData data, Func<AuthCodeEmailTemplateView, (AuthCodeEmailTemplateView, string?)>? enhancer = null, CancellationToken cancellationToken = default)
        {
            var email = data.Email;
            if (!MailAddress.TryCreate(email, out var emailAddress))
            {
                return ApplicationErrors.InvalidEmail.AsResult();
            }

            // Action
            var action = Actions.Find(a => a.Id == data.Action);
            if (action == null || action.Template == null)
            {
                return ApplicationErrors.NoValidData.AsResult("Action");
            }

            // Login required
            if (action.LoginRequired && User == null)
            {
                return ApplicationErrors.AccessDenied.AsResult();
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
                    Id = authCode.Id,
                    Action = action,
                    Code = code,
                    Language = CultureInfo.CurrentCulture.Name,
                    TimeZone = tz,
                    LocalExpiry = authCode.Expiry.UtcToLocal(tz)
                };

                if (enhancer != null)
                {
                    var (enhancedModel, json) = enhancer(dataModel);
                    dataModel = enhancedModel;
                    authCode.Data = json;
                }

                // Template
                var file = Path.Join(_root, action.Template);
                var template = await RazorUtils.RenderAsync(file, dataModel);

                // Message
                var message = new SendEmailMessage
                {
                    Subject = dataModel.Subject ?? action.Name,
                    Body = template,
                    To = [emailAddress.ToString()]
                };

                await _queueService.PushAsync(message, PlatformSharedContext.Default.SendEmailMessage, cancellationToken);

                // Save
                result = await AddAuthCodeAsync(authCode, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log for trace
                ex.Data.Add("Model", JsonSerializer.Serialize(data, MyJsonSerializerContext.Default.SendSMSData));
                ex.Data.Add("Action", JsonSerializer.Serialize(action, MyJsonSerializerContext.Default.AuthCodeActionItem));

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
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> SendSMSAsync(SMSCodeRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            // Check device
            if (!this.CheckDevice(userAgent, rq.DeviceId, out var checkResult, out var cd))
            {
                return checkResult;
            }

            var deviceCore = cd.Value.DeviceCore;

            var mobile = DecryptDeviceData(rq.Mobile, deviceCore);
            if (mobile == null)
            {
                return ApplicationErrors.NoValidData.AsResult("Mobile");
            }

            var data = new SendSMSData
            {
                Action = rq.Action,
                Mobile = mobile,
                Region = rq.Region
            };

            return await SendSMSAsync(data, null, cancellationToken);
        }

        /// <summary>
        /// Send SMS code
        /// 发送短信验证码
        /// </summary>
        /// <typeparam name="D">Generic json data type</typeparam>
        /// <param name="data">Data</param>
        /// <param name="typeInfo">JSON type info</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> SendSMSAsync<D>(SendSMSData<D> data, JsonTypeInfo<D> typeInfo, CancellationToken cancellationToken = default) where D : AuthCodeData
        {
            return await SendSMSAsync(data,
                (action) => action.Data = JsonSerializer.Serialize(data.Data, typeInfo),
                cancellationToken);
        }

        /// <summary>
        /// Send SMS code
        /// 发送短信验证码
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="enhancer">Data enhancer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> SendSMSAsync(SendSMSData data, Action<CoreAuthCode>? enhancer = null, CancellationToken cancellationToken = default)
        {
            // Mobile
            var region = data.Region;
            var mobile = AddressRegion.CreatePhone(data.Mobile, region);
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

            // Login required
            if (action.LoginRequired && User == null)
            {
                return ApplicationErrors.AccessDenied.AsResult();
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

            // Enhance data
            enhancer?.Invoke(authCode);

            // Send
            await _queueService.PushAsync(new SendSMSMessage
            {
                Kind = SendSMSMessage.KindCode,
                Culture = CultureInfo.CurrentCulture.Name,
                Region = region,
                To = [mobile],
                Body = code
            }, PlatformSharedContext.Default.SendSMSMessage, cancellationToken);

            // Save
            result = await AddAuthCodeAsync(authCode, cancellationToken);

            // Return
            return result;
        }

        private string HashAuthCode(AuthCodeAction id, string code, DateTime expiry)
        {
            return App.HashPassword($"{(short)id}-{code}-{expiry.ToBinary()}");
        }

        private CoreAuthCode CreateAuthCode(AuthCodeActionItem action, string openid, out string code)
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
                Ip= _ip,
                CoreUserId = User?.IdInt
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

        private async Task<IActionResult> CheckFrequencyAsync(AuthCodeActionItem action, string openid, CancellationToken cancellationToken)
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
        /// Read code
        /// 读取验证码
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ValidateResultData?> ReadAsync(Guid id, AuthCodeAction action, CancellationToken cancellationToken = default)
        {
            var code = await _db.CoreAuthCodes.AsNoTracking()
                .Where(c => c.Id == id && c.Action == action)
                .Select(c => new ValidateResultData
                {
                    OpenId = c.Openid,
                    Expiry = c.Expiry,
                    UserId = c.CoreUserId,
                    Data = c.Data
                })
                .FirstOrDefaultAsync(cancellationToken);

            return code;
        }

        /// <summary>
        /// Validate code
        /// 验证验证码
        /// </summary>
        /// <param name="actionId">Action id</param>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<(ActionResult result, ValidateResultData? data)> ValidateAsync(AuthCodeAction actionId, ValidateCodeData data, CancellationToken cancellationToken = default)
        {
            // Auth code
            var code = await _db.CoreAuthCodes.FirstOrDefaultAsync(c => c.Id == data.Id && c.Action == actionId, cancellationToken);
            if (code == null || code.Expiry < DateTime.UtcNow)
            {
                // Deleted or expired
                return (ApplicationErrors.CodeExpired.AsResult(), null);
            }

            if (!code.Ip.Equals(_ip))
            {
                // IP address changed
                return (ApplicationErrors.IPAddressChanged.AsResult(), null);
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
                return (ApplicationErrors.CodesNoMatch.AsResult(), null);
            }

            // Delete
            _db.CoreAuthCodes.Remove(code);
            await _db.SaveChangesAsync(cancellationToken);

            // Result data
            var resultData = new ValidateResultData
            {
                OpenId = code.Openid,
                Expiry = code.Expiry,
                UserId = code.CoreUserId,
                Data = code.Data
            };

            return (ActionResult.Success, resultData);
        }
    }
}
