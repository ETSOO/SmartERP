using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.FinanceTransaction
{
    /// <summary>
    /// Finance transaction currency exchange request data
    /// 财务交易货币兑换请求数据
    /// </summary>
    public record FinanceTransactionCurrencyExchangeRQ : IModelValidator
    {
        /// <summary>
        /// Source account id
        /// 转出账户编号
        /// </summary>
        public int SourceAccountId { get; init; }

        /// <summary>
        /// Destination account id
        /// 转入账户编号
        /// </summary>
        public int DestinationAccountId { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Exchange rate
        /// 汇率
        /// </summary>
        public decimal ExchangeRate { get; init; }

        /// <summary>
        /// Exchange amount
        /// 兑换金额
        /// </summary>
        public decimal ExchangeAmount { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Reference id
        /// 参考编号
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (DestinationAccountId == SourceAccountId)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(DestinationAccountId));
            }

            if (Amount <= 0)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Amount));
            }

            if (ExchangeRate <= 0)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(ExchangeRate));
            }

            if (ExchangeAmount <= 0 || Math.Abs(ExchangeAmount - Amount * ExchangeRate) > 1)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(ExchangeAmount));
            }

            if (ReferenceId != null && ReferenceId.Length is not (>= 1 and <= 30))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(ReferenceId));
            }

            return null;
        }
    }
}
