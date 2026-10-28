namespace CRM.Server.RQ.FinanceTransaction
{
    /// <summary>
    /// Finance transaction query request data
    /// 财务交易查询请求数据
    /// </summary>
    public record FinanceTransactionQueryRQ : FinanceTransactionListRQ
    {
        /// <summary>
        /// Creation start
        /// 登记时间开始
        /// </summary>
        public DateTimeOffset? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 登记时间结束
        /// </summary>
        public DateTimeOffset? CreationEnd { get; init; }

        /// <summary>
        /// Happen date start
        /// 发生日期开始
        /// </summary>
        public DateTimeOffset? HappenDateStart { get; init; }

        /// <summary>
        /// Happen date end
        /// 发生日期结束
        /// </summary>
        public DateTimeOffset? HappenDateEnd { get; init; }
    }
}
