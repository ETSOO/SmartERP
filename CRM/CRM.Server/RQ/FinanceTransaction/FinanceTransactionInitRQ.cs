using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.FinanceTransaction
{
    /// <summary>
    /// Finance transaction init request data
    /// 财务交易初始化请求数据
    /// </summary>
    public record FinanceTransactionInitRQ : IModelValidator
    {
        /// <summary>
        /// Account id
        /// 账户编号
        /// </summary>
        public int AccountId { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Amount == 0)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Amount));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            return null;
        }
    }
}
