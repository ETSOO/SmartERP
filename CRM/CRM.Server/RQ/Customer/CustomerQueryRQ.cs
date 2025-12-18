namespace CRM.Server.RQ.Customer
{
    /// <summary>
    /// Customer query request data
    /// 客户查询请求数据
    /// </summary>
    public record CustomerQueryRQ : CustomerListRQ
    {
        /// <summary>
        /// Assigned ID
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Contact information
        /// 联系信息
        /// </summary>
        public string? Info { get; init; }
    }
}