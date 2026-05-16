using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.Dto.Stock;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Check stock request data
    /// 检查库存请求数据
    /// </summary>
    public record CheckStockRQ : IModelValidator
    {
        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int LocationId { get; init; }

        /// <summary>
        /// Items
        /// 类型
        /// </summary>
        public required IEnumerable<StockItem> Items { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (!Items.Any() || Items.Any(i => i.Qty <= 0) || !Items.IsProductUnique())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Items));
            }

            return null;
        }
    }
}
