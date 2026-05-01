namespace CRM.Server.RQ.POLine
{
    /// <summary>
    /// Purchase line query all request data
    /// 查询所有采购行请求数据
    /// </summary>
    public record POLineQueryAllRQ : POLineListRQ
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
        /// Qty start
        /// 开始数据
        /// </summary>
        public decimal? QtyStart { get; init; }

        /// <summary>
        /// Creation start
        /// 创建开始
        /// </summary>
        public DateTimeOffset? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 创建结束
        /// </summary>
        public DateTimeOffset? CreationEnd { get; init; }

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
