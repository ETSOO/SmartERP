namespace CRM.Server.RQ.Supplier
{
    /// <summary>
    /// Supplier query request data
    /// 供应商查询请求数据
    /// </summary>
    public record SupplierQueryRQ : SupplierListRQ
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
