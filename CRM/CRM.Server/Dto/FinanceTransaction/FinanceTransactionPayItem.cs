namespace CRM.Server.Dto.FinanceTransaction
{
    /// <summary>
    /// Pay item
    /// 付款项目
    /// </summary>
    public record FinanceTransactionPayItem
    {
        /// <summary>
        /// Finance account id
        /// 财务账户编号
        /// </summary>
        public int AccountId { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }
    }
}
