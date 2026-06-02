using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Send profile email message
    /// 发送档案邮件信息
    /// </summary>
    public record SendProfileEmailMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "SendProfileEmail";

        /// <summary>
        /// Related target
        /// 关联对象
        /// </summary>
        public required string RelatedTarget { get; init; }

        /// <summary>
        /// Persons
        /// 人员编号
        /// </summary>
        public required IEnumerable<long> Persons { get; init; }

        /// <summary>
        /// Message
        /// 留言
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Include attachments or not
        /// 是否包含附件
        /// </summary>
        public bool? IncludeAttachments { get; init; }

        /// <summary>
        /// Include comments or not
        /// 是否包含评论
        /// </summary>
        public bool? IncludeComments { get; init; }

        /// <summary>
        /// Recipients
        /// 收件人
        /// </summary>
        public required string[] Emails { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(RelatedTarget)] = RelatedTarget,
            [nameof(Persons)] = Persons,
            [nameof(Emails)] = Emails,
            [nameof(Message)] = Message,
            [nameof(IncludeAttachments)] = IncludeAttachments,
            [nameof(IncludeComments)] = IncludeComments
        };
    }
}
