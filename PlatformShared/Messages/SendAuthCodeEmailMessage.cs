using com.etsoo.Utils.Serialization;
using PlatformShared.Dto;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Sending authentication code email message
    /// 发送验证码电子邮件消息
    /// </summary>
    public record SendAuthCodeEmailMessage : IMessageQueueMessage
    {
        public static string Type => "SendAuthCodeEmail";

        /// <summary>
        /// Template model
        /// 模板模型
        /// </summary>
        public required AuthCodeEmailTemplateView Model { get; init; }

        /// <summary>
        /// Recipients
        /// 收件人
        /// </summary>
        public required IEnumerable<string> To { get; init; }
    }
}
