using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.OrderLine
{
    /// <summary>
    /// Complete execution request data
    /// 完成执行请求数据
    /// </summary>
    public record OrderLineCompleteRQ : IModelValidator
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Asset id
        /// 资产编号
        /// </summary>
        public int? AssetId { get; init; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long? SupplierId { get; init; }

        /// <summary>
        /// Cost price
        /// 成本价
        /// </summary>
        public decimal? CostPrice { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (SupplierId.HasValue && (!CostPrice.HasValue || CostPrice.Value < 0))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(CostPrice));
            }

            return null;
        }
    }
}
