namespace CRM.Server.RQ
{
    /// <summary>
    /// Document action request data
    /// 文档操作请求数据
    /// </summary>
    public record DocumentActionRQ
    {
        /// <summary>
        /// Document id
        /// 文档编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Related target id
        /// 关联对象编号
        /// </summary>
        public long TargetId { get; init; }
    }
}
