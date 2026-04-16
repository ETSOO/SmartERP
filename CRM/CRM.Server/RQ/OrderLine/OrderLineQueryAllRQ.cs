namespace CRM.Server.RQ.OrderLine
{
    /// <summary>
    /// Order line query all request data
    /// 查询所有订单行请求数据
    /// </summary>
    public record OrderLineQueryAllRQ : OrderLineListRQ
    {
        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// Source
        /// 来源
        /// </summary>
        public string? Source { get; init; }

        /// <summary>
        /// Customer id
        /// 客户编号
        /// </summary>
        public long? CustomerId { get; init; }

        /// <summary>
        /// Qty start
        /// 开始数据
        /// </summary>
        public decimal? QtyStart { get; init; }

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
