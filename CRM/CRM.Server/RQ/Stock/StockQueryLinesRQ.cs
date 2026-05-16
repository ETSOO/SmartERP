namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Stock query lines request data
    /// 库存行查询请求数据
    /// </summary>
    public record StockQueryLinesRQ : QueryLongRQ
    {
        /// <summary>
        /// Stock id
        /// 库存编号
        /// </summary>
        public long StockId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// Qty start
        /// 开始数量
        /// </summary>
        public decimal? QtyStart { get; init; }
    }
}
