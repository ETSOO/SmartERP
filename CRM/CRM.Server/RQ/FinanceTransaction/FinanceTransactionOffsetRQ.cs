using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.FinanceTransaction
{
    /// <summary>
    /// Finance transaction offset request data
    /// 财务交易冲销请求数据
    /// </summary>
    public record FinanceTransactionOffsetRQ : IModelValidator
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

        /// <summary>
        /// All items at the same time
        /// 所有项目同时处理
        /// </summary>
        public bool? All { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Description.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            return null;
        }
    }
}
