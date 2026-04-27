using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;

namespace CRM.Server.RQ.Product
{
    /// <summary>
    /// Query products for purchase request data
    /// 查询产品用于采购的请求数据
    /// </summary>
    public record QueryForPurchaseRQ : QueryIntRQ, IModelValidator
    {
        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public required long SupplierId { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public string? Culture { get; init; }

        /// <summary>
        /// Category
        /// 所属分类
        /// </summary>
        public int? CategoryId { get; init; }

        /// <summary>
        /// Category and all descendant category ids
        /// 所属分类及所有下级子类编号
        /// </summary>
        public int? CategoryIdAll { get; init; }

        /// <summary>
        /// Assigned id start
        /// 分配的编号开始
        /// </summary>
        public string? AssignedIdStart { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public override IActionResult? Validate()
        {
            var result = base.Validate();
            if (result != null && !result.Ok)
            {
                return result;
            }

            if (!new CurrencyAttribute().IsValid(Currency))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Currency));
            }

            if (!new LanguageCodeAttribute().IsValid(Culture))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Culture));
            }

            if (Keyword != null && Keyword.Length is not (> 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Keyword));
            }

            if (AssignedIdStart != null && AssignedIdStart.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedIdStart));
            }

            return null;
        }
    }
}
