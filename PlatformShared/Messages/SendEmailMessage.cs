using com.etsoo.MessageQueue;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Email importance
    /// 邮件重要性
    /// </summary>
    public enum EmailImportance
    {
        Low,
        Normal,
        High
    }

    /// <summary>
    /// Email priority
    /// 邮件优先级
    /// </summary>
    public enum EmailPriority
    {
        NonUrgent,
        Normal,
        Urgent
    }

    /// <summary>
    /// Send email message
    /// 发送邮件消息
    /// </summary>
    public record SendEmailMessage : IMessageQueueMessage
    {
        public static string Type => "SendEmail";

        /// <summary>
        /// Subject
        /// 主题
        /// </summary>
        public required string Subject { get; init; }

        /// <summary>
        /// Body
        /// 内容
        /// </summary>
        public required string Body { get; init; }

        /// <summary>
        /// Recipients
        /// 收件人
        /// </summary>
        public required IEnumerable<string> To { get; init; }

        /// <summary>
        /// CC
        /// 抄送人
        /// </summary>
        public IEnumerable<string>? Cc { get; init; }

        /// <summary>
        /// BCC
        /// 密送人
        /// </summary>
        public IEnumerable<string>? Bcc { get; init; }

        /// <summary>
        /// Importance
        /// 重要性
        /// </summary>
        public EmailImportance? Importance { get; init; }

        /// <summary>
        /// Priority
        /// 优先级
        /// </summary>
        public EmailPriority? Priority { get; init; }
    }
}
