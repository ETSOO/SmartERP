namespace CRM.Server.RQ.StockSite
{
    /// <summary>
    /// Stock site query request data
    /// 库存点查询请求数据
    /// </summary>
    public record StockSiteQueryRQ : QueryLongRQ
    {
        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int? LocationId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// Refresh time start
        /// 刷新时间开始
        /// </summary>
        public DateTimeOffset? RefreshTimeStart { get; init; }

        /// <summary>
        /// Refresh time end
        /// 刷新时间结束
        /// </summary>
        public DateTimeOffset? RefreshTimeEnd { get; init; }
    }
}
