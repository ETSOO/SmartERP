using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Stock update line request data
    /// 库存更新行请求数据
    /// </summary>
    public record StockUpdateLineRQ : IModelValidator
    {
        /// <summary>
        /// Line id
        /// 行编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// New qty
        /// 新数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Qty < 0)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Qty));
            }

            return null;
        }
    }
}