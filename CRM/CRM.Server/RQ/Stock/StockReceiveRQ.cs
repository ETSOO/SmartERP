using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Stock receive request data
    /// 入库请求数据
    /// </summary>
    public record StockReceiveRQ : IModelValidator
    {
        /// <summary>
        /// Stock id
        /// 库存编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Tracking number
        /// 物流编号
        /// </summary>
        public string? TrackingNumber { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (TrackingNumber != null && TrackingNumber.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(TrackingNumber));
            }

            return null;
        }
    }
}
