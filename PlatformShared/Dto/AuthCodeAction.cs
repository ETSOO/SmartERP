using com.etsoo.Utils.Crypto;
using PlatformShared.Database.Models;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Authentication code action item
    /// 验证码操作项目
    /// </summary>
    /// <param name="Id">Id</param>
    /// <param name="Minutes">Valid minutes</param>
    /// <param name="Kind">Kind</param>
    /// <param name="Length">Length</param>
    /// <param name="LoginRequired">Login required</param>
    /// <param name="Template">Template path</param>
    public record AuthCodeActionItem(
        AuthCodeAction Id,
        short Minutes,
        RandStringKind Kind,
        byte Length,
        bool LoginRequired = false,
        string? Template = null
    )
    {
        /// <summary>
        /// Code actions
        /// 验证码操作
        /// </summary>
        public static AuthCodeActionItem[] Actions =>
        [
            new(AuthCodeAction.UserRegistrationSMSCode, 10, RandStringKind.Digit, 6),
            new(AuthCodeAction.UserRegistrationEmailCode, 30, RandStringKind.Digit, 6, false, "/Templates/EmailRegistration_{culture}.cshtml"),
            new(AuthCodeAction.UserCallbackSMSCode, 10, RandStringKind.Digit, 6),
            new(AuthCodeAction.UserCallbackEmailCode, 30, RandStringKind.Digit, 6, false, "/Templates/EmailCallback_{culture}.cshtml"),
            new(AuthCodeAction.UserVerificationSMSCode, 10, RandStringKind.Digit, 6, true),
            new(AuthCodeAction.UserVerificationEmailCode, 30, RandStringKind.Digit, 6, true, "/Templates/EmailVerification_{culture}.cshtml"),

            // Member invitation, 3 days = 72 hours = 4320 minutes
            new(AuthCodeAction.MemberInvitationEmailCode, 4320, RandStringKind.DigitAndLetter, 16, true, "/Templates/EmailMemberInvitation_{culture}.cshtml")
        ];
    }
}
