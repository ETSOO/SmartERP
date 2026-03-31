using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;

namespace CRM.Server.RQ.Customer
{
    /// <summary>
    /// Read customer data for sale request data
    /// 读取销售用的客户数据请求
    /// </summary>
    public record CustomerReadForSaleRQ : IModelValidator
    {
        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Customer id, null or 0 for anonymous customer
        /// 客户编号，null或者0表示匿名客户
        /// </summary>
        public long? CustomerId { get; init; }

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
