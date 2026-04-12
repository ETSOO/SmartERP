namespace CRM.Server.RQ.OrderLine
{
    /// <summary>
    /// Order line query request data
    /// 订单行查询请求数据
    /// </summary>
    public record OrderLineQueryRQ : OrderLineListRQ
    {
        /// <summary>
        /// Start time start
        /// 开始时间开始
        /// </summary>
        public DateTimeOffset? StartTimeStart { get; init; }

        /// <summary>
        /// Start time end
        /// 开始时间结束
        /// </summary>
        public DateTimeOffset? StartTimeEnd { get; init; }
    }
}
