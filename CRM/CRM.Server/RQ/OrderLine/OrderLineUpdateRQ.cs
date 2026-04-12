using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using System.Text.Json;

namespace CRM.Server.RQ.OrderLine
{
    /// <summary>
    /// Update order line request data
    /// 更新订单行请求数据
    /// </summary>
    public record OrderLineUpdateRQ : UpdateModel<long>, IModelValidator
    {
        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal? Qty { get; init; }

        /// <summary>
        /// Price
        /// 价格
        /// </summary>
        public decimal? Price { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Start time
        /// 开始时间
        /// </summary>
        public DateTimeOffset? StartTime { get; init; }

        /// <summary>
        /// End time
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndTime { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public JsonDocument? Data { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Qty.HasValue && (Qty.Value <= 0 || Qty.Value > 999999999 || Qty.Value.Scale > 2))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Qty));
            }

            if (Price.HasValue && Price.Value < 0)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Price));
            }

            if (Title != null && Title.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 1280))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            return null;
        }
    }
}
