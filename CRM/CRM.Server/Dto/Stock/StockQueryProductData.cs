namespace CRM.Server.Dto.Stock
{
    /// <summary>
    /// Stock query product data
    /// 库存查询产品数据
    /// </summary>
    public record StockQueryProductData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Stock qty
        /// 库存数量
        /// </summary>
        public decimal? Qty { get; init; }

        /// <summary>
        /// Unit name
        /// 单位名称
        /// </summary>
        public required string UnitName { get; init; }
    }
}
