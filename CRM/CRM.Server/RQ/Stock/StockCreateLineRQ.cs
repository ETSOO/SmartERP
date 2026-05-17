using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Create stock line request data
    /// 创建库存行请求数据
    /// </summary>
    public record StockCreateLineRQ : IModelValidator
    {
        /// <summary>
        /// Stock id
        /// 库存编号
        /// </summary>
        public long StockId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Order / PO line id
        /// 订单/采购行编号
        /// </summary>
        public long OrderLineId { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Qty <= 0)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Qty));
            }

            return null;
        }
    }
}
