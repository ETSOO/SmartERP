namespace CRM.Server.RQ.FinanceTransaction
{
    /// <summary>
    /// Finance transaction offset request data
    /// 财务交易冲销请求数据
    /// </summary>
    public record FinanceTransactionOffsetRQ
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public required string Description { get; init; }
    }
}
