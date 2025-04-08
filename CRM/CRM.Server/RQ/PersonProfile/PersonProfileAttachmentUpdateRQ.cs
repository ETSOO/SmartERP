namespace CRM.Server.RQ.PersonProfile
{
    /// <summary>
    /// Person profile attachment update request data
    /// 人员档案附件更新请求数据
    /// </summary>
    public record PersonProfileAttachmentUpdateRQ
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public required long Id { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public required string Description { get; init; }
    }
}
