using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.Localization;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using System.Net.Mail;

namespace Platform.Server.Endpoints.Member.RQ
{
    /// <summary>
    /// Member invite request data
    /// 成员邀请请求数据
    /// </summary>
    public record MemberInviteRQ : IModelValidator
    {
        /// <summary>
        /// User role
        /// 用户角色
        /// </summary>
        public UserRole UserRole { get; init; }

        /// <summary>
        /// Emails
        /// 电子信箱
        /// </summary>
        public required IEnumerable<string> Emails { get; init; }

        /// <summary>
        /// Message
        /// 留言
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Current's time zone
        /// 所在时区
        /// </summary>
        public required string TimeZone { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (!Emails.Any() || Emails.Any(email => !MailAddress.TryCreate(email, out _)))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Emails));
            }

            if (Message != null && Message.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Message));
            }

            if (!LocalizationUtils.IsTimeZone(TimeZone))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(TimeZone));
            }

            return null;
        }
    }
}
