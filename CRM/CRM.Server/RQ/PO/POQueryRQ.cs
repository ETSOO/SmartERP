namespace CRM.Server.RQ.PO
{
    /// <summary>
    /// PO query request data
    /// 订单查询请求数据
    /// </summary>
    public record POQueryRQ : POListRQ
    {
        /// <summary>
        /// Source id
        /// 来源编号
        /// </summary>
        public string? SourceId { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Has promotion or not
        /// 是否有促销活动
        /// </summary>
        public bool? HasPromotion { get; init; }

        /// <summary>
        /// Creation start
        /// 创建开始时间
        /// </summary>
        public DateTimeOffset? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 创建结束时间
        /// </summary>
        public DateTimeOffset? CreationEnd { get; init; }

        /// <summary>
        /// Start date start
        /// 开始时间开始
        /// </summary>
        public DateTimeOffset? StartDateStart { get; init; }

        /// <summary>
        /// Start date end
        /// 开始时间结束
        /// </summary>
        public DateTimeOffset? StartDateEnd { get; init; }

        /// <summary>
        /// Amount start
        /// 金额起始
        /// </summary>
        public decimal? AmountStart { get; init; }

        /// <summary>
        /// Amount end
        /// 金额结束
        /// </summary>
        public decimal? AmountEnd { get; init; }
    }
}
