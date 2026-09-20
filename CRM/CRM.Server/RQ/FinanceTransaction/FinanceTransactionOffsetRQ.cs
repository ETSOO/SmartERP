namespace CRM.Server.RQ.FinanceTransaction
{
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
