namespace CRM.Server.Dto.Supplier
{
    /// <summary>
    /// Supplier query data
    /// 供应商查询数据
    /// </summary>
    public record SupplierQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }
    }
}
