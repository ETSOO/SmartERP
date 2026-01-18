using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using PlatformShared.Dto;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Promotion
{
    /// <summary>
    /// Update promotion request data
    /// 更新促销请求数据
    /// </summary>
    public record PromotionUpdateRQ : UpdateModel<int>, IModelValidator
    {
        /// <summary>
        /// Promotion code
        /// 促销代码
        /// </summary>
        [JsonConverter(typeof(PromotionCodeConverter))]
        public PromotionCode? Code { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string? Currency { get; init; }

        /// <summary>
        /// Related product ids
        /// 关联的产品编号
        /// </summary>
        public IEnumerable<int>? ProductIds { get; set; }

        /// <summary>
        /// Related product category ids
        /// 关联的产品类目编号
        /// </summary>
        public IEnumerable<int>? ProductCategoryIds { get; set; }

        /// <summary>
        /// Related person (customer) ids
        /// 关联的人员（客户）编号
        /// </summary>
        public IEnumerable<long>? PersonIds { get; set; }

        /// <summary>
        /// Related person category ids
        /// 关联的人员类目编号
        /// </summary>
        public IEnumerable<int>? PersonCategoryIds { get; set; }

        /// <summary>
        /// Minimum spending amount
        /// 最低消费金额
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// Discount percentage, like 10 for 10%
        /// 折扣百分比，如 10 代表 10%
        /// </summary>
        public int? Discount { get; set; }

        /// <summary>
        /// Valid start date
        /// 有效开始时间
        /// </summary>
        public DateTimeOffset? ValidStart { get; init; }

        /// <summary>
        /// Valid start end
        /// 有效结束时间
        /// </summary>
        public DateTimeOffset? ValidEnd { get; init; }

        /// <summary>
        /// Max coupons
        /// 最大优惠券
        /// </summary>
        public int? Coupons { get; init; }

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
        /// Order index
        /// 排序数
        /// </summary>
        public short? OrderIndex { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Title != null && Title.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (Currency != null && !new CurrencyAttribute().IsValid(Currency))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Currency));
            }

            return null;
        }
    }
}