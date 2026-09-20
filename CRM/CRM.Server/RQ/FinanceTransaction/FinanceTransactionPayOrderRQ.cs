namespace CRM.Server.RQ.FinanceTransaction
{
    /// <summary>
    /// Pay order request data
    /// 订单付款请求数据
    /// </summary>
    public record FinanceTransactionPayOrderRQ
    {
        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long OrderId { get; init; }


    }
}
  