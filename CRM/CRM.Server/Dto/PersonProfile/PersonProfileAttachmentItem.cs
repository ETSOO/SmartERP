namespace CRM.Server.Dto.PersonProfile
{
    /// <summary>
    /// Person profile attachment item
    /// 人员档案附件项
    /// </summary>
    public record PersonProfileAttachmentItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// File size
        /// 文件大小
        /// </summary>
        public long FileSize { get; init; }

        /// <summary>
        /// Content type
        /// 文件类型
        /// </summary>
        public required string ContentType { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public required string Description { get; init; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long UserId { get; init; }

        /// <summary>
        /// User name
        /// 用户姓名
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }

        /// <summary>
        /// Is the author of self
        /// 自己是否为作者
        /// </summary>
        public bool IsSelf { get; init; }
    }
}
