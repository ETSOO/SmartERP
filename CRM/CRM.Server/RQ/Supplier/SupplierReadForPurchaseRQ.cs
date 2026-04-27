using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.WebUtils.Attributes;

namespace CRM.Server.RQ.Supplier
{
    /// <summary>
    /// Read supplier data for purchase request data
    /// 读取采购用的供应商数据请求
    /// </summary>
    public record SupplierReadForPurchaseRQ
    {
        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Supplier id, null or 0 for anonymous supplier
        /// 供应商编号，null或者0表示匿名供应商
        /// </summary>
        public long? SupplierId { get; init; }

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
