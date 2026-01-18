using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using PlatformShared.Dto;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Promotion
{
    /// <summary>
    /// Create promotion request data
    /// 创建促销请求数据
    /// </summary>
    public record PromotionCreateRQ : PromotionCodeData, IModelValidator
    {
        /// <summary>
        /// Promotion code
        /// 促销代码
        /// </summary>
        [JsonConverter(typeof(PromotionCodeConverter))]
        public required PromotionCode Code { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Valid start date
        /// 有效开始时间
        /// </summary>
        public DateTimeOffset ValidStart { get; init; }

        /// <summary>
        /// Valid start end
        /// 有效结束时间
        /// </summary>
        public DateTimeOffset ValidEnd { get; init; }

        /// <summary>
        /// Max coupons
        /// 最大优惠券
        /// </summary>
        public int Coupons { get; init; }

        /// <summary>
        /// Stackable
        /// 促销是否可叠加
        /// </summary>
        public bool? Stackable { get; init; }

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
            var field = Code.Check(this);
            if (!string.IsNullOrEmpty(field))
            {
                return new ActionResult
                {
                    Title = $"Code validatiaon failed with {field}"
                };
            }

            if (Title.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (!new CurrencyAttribute().IsValid(Currency))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Currency));
            }

            return null;
        }
    }
}
