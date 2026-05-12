namespace CRM.Server.Dto.StockSite
{
    /// <summary>
    /// Stock site query data
    /// 库存点查询数据
    /// </summary>
    public record StockSiteQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

        /// <summary>
        /// Product name
        /// 产品名称
        /// </summary>
        public required string ProductName { get; init; }

        /// <summary>
        /// Qty.
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Last refresh time
        /// 上次刷新时间
        /// </summary>
        public DateTimeOffset RefreshTime { get; init; }
    }
}
