namespace CRM.Server.Dto.Supplier
{
    /// <summary>
    /// Supplier list data
    /// 供应商列表数据
    /// </summary>
    public record SupplierListData
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

        /// <summary>
        /// Preferred Name
        /// 首选名称
        /// </summary>
        public string? PreferredName { get; init; }
    }
}
