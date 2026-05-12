namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Stock query request
    /// 库存查询请求
    /// </summary>
    public record StockQueryRQ : StockListRQ
    {
        /// <summary>
        /// Total qty start
        /// 总数量开始
        /// </summary>
        public decimal? TotalQtyStart { get; init; }

        /// <summary>
        /// Total qty end
        /// 总数量结束
        /// </summary>
        public decimal? TotalQtyEnd { get; init; }

        /// <summary>
        /// Total qty end
        /// Creation start
        /// 登记日期开始
        /// </summary>
        public DateTimeOffset? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 登记日期结束
        /// </summary>
        public DateTimeOffset? CreationEnd { get; init; }
    }
}
