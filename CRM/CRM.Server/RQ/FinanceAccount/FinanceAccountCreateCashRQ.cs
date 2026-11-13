using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;

namespace CRM.Server.RQ.FinanceAccount
{
    /// <summary>
    /// Create cash account request data
    /// 创建现金账户请求数据
    /// </summary>
    public record FinanceAccountCreateCashRQ : IModelValidator
    {
        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (!new CurrencyAttribute().IsValid(Currency))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Currency));
            }

            return null;
        }
    }
}
