namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Person profile send email request data
    /// 人员档案发送邮件请求数据
    /// </summary>
    public class SendProfileEmailRQ
    {
        /// <summary>
        /// Profile ID
        /// 档案编号
        /// </summary>
        public long Id { get; init; }

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
    }
}
