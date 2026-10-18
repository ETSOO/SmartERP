using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.Dto.FinanceTransaction;

namespace CRM.Server.RQ.FinanceTransaction
{
    /// <summary>
    /// Finance transaction reimburse request data
    /// 财务交易报销请求数据
    /// </summary>
    public record FinanceTransactionReimburseRQ : IModelValidator
    {
        /// <summary>
        /// Reimbursement person id
        /// 报销人人员编号
        /// </summary>
        public required long TargetPersonId { get; init; }

        /// <summary>
        /// Reimbursement person account id
        /// 报销人账户
        /// </summary>
        public int? TargetAccountId { get; init; }

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
        /// Reference id
        /// 参考编号
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Order / PO id
        /// 订单或采购订单编号
        /// </summary>
        public long? OrderId { get; init; }

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
