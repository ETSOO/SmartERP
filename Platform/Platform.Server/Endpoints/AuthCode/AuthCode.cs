using com.etsoo.WebUtils;
using Platform.Server.Endpoints.AuthCode.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.AuthCode
{
    /// <summary>
    /// Auth code APIs
    /// 验证码接口
    /// </summary>
    public static class AuthCode
    {
        public static RouteGroupBuilder MapAuthCode(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("AuthCode").AllowAnonymous();

            g.MapPut("SendEmail", (IAuthCodeService service, IHttpContextAccessor accessor, EmailCodeRQ rq, CancellationToken cancellationToken) => service.SendEmailAsync(rq, accessor.UserAgent(), cancellationToken))
            .WithDescription("Send Email code / 发送电子邮箱验证码").WithTags("AuthCode");

            g.MapPut("SendSMS", (IAuthCodeService service, IHttpContextAccessor accessor, SMSCodeRQ rq, CancellationToken cancellationToken) => service.SendSMSAsync(rq, accessor.UserAgent(), cancellationToken))
                .WithDescription("Send SMS code / 发送短信验证码").WithTags("AuthCode");

            return builder;
        }
    }
}
