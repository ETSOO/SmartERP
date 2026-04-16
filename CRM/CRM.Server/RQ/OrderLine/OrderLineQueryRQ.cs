namespace CRM.Server.RQ.OrderLine
{
    /// <summary>
    /// Order line query request data
    /// 订单行查询请求数据
    /// </summary>
    public record OrderLineQueryRQ : OrderLineListRQ
    {
        /// <summary>
        /// Qty start
        /// 开始数据
        /// </summary>
        public decimal? QtyStart { get; init; }
    }
}
