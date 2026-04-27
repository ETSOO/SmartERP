using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Dto;
using System.Text.Json;

namespace CRM.Server.RQ.PO
{
    /// <summary>
    /// PO line request data
    /// 订单项目请求数据
    /// </summary>
    public record POLineRQ : IModelValidator
    {
        /// <summary>
        /// Product ID
        /// 产品编号
        /// </summary>
        public required int ProductId { get; init; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public required decimal Qty { get; init; }

        /// <summary>
        /// Price, to be validated by the backend when value presented
        /// 价格，如果提供后台会验证这个值是否正确
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
        /// Promotions
        /// 促销
        /// </summary>
        public IEnumerable<PromotionSaleItemBase>? Promotions { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus? Status { get; init; }

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

