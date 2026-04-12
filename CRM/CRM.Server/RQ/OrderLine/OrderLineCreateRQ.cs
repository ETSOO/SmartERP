using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Dto;
using System.Text.Json;

namespace CRM.Server.RQ.OrderLine
{
    /// <summary>
    /// Create order line request data
    /// 创建订单行请求数据
    /// </summary>
    public record OrderLineCreateRQ : IModelValidator
    {
        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public required long OrderId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public required int ProductId { get; init; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public required decimal Qty { get; init; }

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
        public JsonDocument? Data { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// Promotions
        /// 促销
        /// </summary>
        public IEnumerable<PromotionSaleItemBase>? Promotions { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Qty <= 0 || Qty > 999999999 || Qty.Scale > 2)
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
