using System.Diagnostics.CodeAnalysis;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Invitation authorization code email template view model
    /// 邀请验证码邮件模板浏览模型
    /// </summary>
    public record InvitationAuthCodeEmailTemplateView : AuthCodeEmailTemplateView
    {
        [SetsRequiredMembers]
        public InvitationAuthCodeEmailTemplateView(AuthCodeEmailTemplateView data, AuthCodeMemberInvitationData obj)
            : base(data)
        {
            DataObj = obj;
        }

        /// <summary>
        /// Invitation data object
        /// 邀请数据对象
        /// </summary>
        public AuthCodeMemberInvitationData DataObj { get; }
    }
}
