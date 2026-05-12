namespace CRM.Server.Dto.StockSite
{
    /// <summary>
    /// Stock site view product data
    /// 库存点浏览产品数据
    /// </summary>
    public record StockSiteViewProductData
    {
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int? LocationId { get; init; }

        /// <summary>
        /// Location name
        /// 位置名称
        /// </summary>
        public string? LocationName { get; init; }

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
