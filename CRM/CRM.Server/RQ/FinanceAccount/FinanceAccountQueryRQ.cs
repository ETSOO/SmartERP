namespace CRM.Server.RQ.FinanceAccount
{
    /// <summary>
    /// Finance account query request data
    /// 财务账户查询请求数据
    /// </summary>
    public record FinanceAccountQueryRQ : FinanceAccountListRQ
    {
        /// <summary>
        /// Times start
        /// 次数开始
        /// </summary>
        public int? TimesStart { get; init; }
        
        /// <summary>
        /// Times end
        /// 次数结束
        /// </summary>
        public int? TimesEnd { get; init; }
    }
}
