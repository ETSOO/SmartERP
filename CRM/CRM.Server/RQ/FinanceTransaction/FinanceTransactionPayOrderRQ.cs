using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.Dto.FinanceTransaction;

namespace CRM.Server.RQ.FinanceTransaction
{
    /// <summary>
    /// Pay order request data
    /// 订单付款请求数据
    /// </summary>
    public record FinanceTransactionPayOrderRQ : IModelValidator
    {
        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long OrderId { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Pay items
        /// 付款项目
        /// </summary>
        public required IEnumerable<FinanceTransactionPayItem> Items { get; init; }

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
            if (Amount <= 0)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Amount));
            }

            var calculatedAmount = 0M;
            foreach (var item in Items)
            {
                if (item.Amount <= 0)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(Items));
                }

                calculatedAmount += item.Amount;
            }

            if (calculatedAmount != Amount)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Amount));
            }

            return null;
        }
    }
}
  