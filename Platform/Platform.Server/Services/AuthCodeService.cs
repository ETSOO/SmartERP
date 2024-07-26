using com.etsoo.SMS;
using com.etsoo.SMTP;
using com.etsoo.Utils.Crypto;
using Platform.Server.Application;
using Platform.Server.Database;
using Platform.Server.Dto.AuthCode;

namespace Platform.Server.Services
{
    /// <summary>
    /// Authentication code service
    /// 认证码服务
    /// </summary>
    public class AuthCodeService : CommonService
    {
        // Code actions
        private static List<AuthCodeAction> Actions => new()
        {
            new(1, Properties.Resources.UserRegistrationSMSCode, 10, RandStringKind.Digit, 4),
            new(2, Properties.Resources.UserRegistrationEmailCode, 30, RandStringKind.Digit, 4, "/Templates/EmailRegistration.cshtml"),
            new(3, Properties.Resources.UserCallbackSMSCode, 10, RandStringKind.Digit, 6),
            new(4, Properties.Resources.UserCallbackEmailCode, 30, RandStringKind.Digit, 6, "/Templates/EmailCallback.cshtml"),
            new(5, Properties.Resources.UserRegistrationSMSCode, 10, RandStringKind.Digit, 4),
            new(6, Properties.Resources.UserRegistrationEmailCode, 30, RandStringKind.Digit, 4, "/Templates/EmailVerification.cshtml"),
        };

        readonly ISMSClient _smsClient;
        readonly ISMTPClient _smtpClient;
        readonly string _root;

        private readonly MyDbContext _db;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        /// <param name="httpClientFactory">HTTP client factory</param>
        public AuthCodeService(MyDbContext db, IMyApp app, IMyUserAccessor userAccessor, ILogger<AuthService> logger,
                ISMSClient smsClient, ISMTPClient smtpClient, IWebHostEnvironment host
            )
            : base(app, userAccessor.User, "auth_code", logger)
        {
            _db = db;

            _smsClient = smsClient;
            _smtpClient = smtpClient;
            _root = host.ContentRootPath;
        }
    }
}
