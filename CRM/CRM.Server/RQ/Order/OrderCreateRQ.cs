using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using PlatformShared.Dto;
using System.Text.Json;

namespace CRM.Server.RQ.Order
{
    /// <summary>
    /// Create create request data
    /// 创建订单请求数据
    /// </summary>
    public record OrderCreateRQ : IModelValidator
    {
        /// <summary>
        /// Source
        /// 来源
        /// </summary>
        public string? Source { get; init; }

        /// <summary>
        /// Source id
        /// 来源编号
        /// </summary>
        public string? SourceId { get; init; }

        /// <summary>
        /// Customer id
        /// 客户编号
        /// </summary>
        public required long CustomerId { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public required string Culture { get; init; }

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
        /// Payment way
        /// 付款方式
        /// </summary>
        public int? PaymentId { get; set; }

        /// <summary>
        /// Payment instruction
        /// 付款指示
        /// </summary>
        public string? PaymentInstruction { get; init; }

        /// <summary>
        /// Delivery way
        /// 发货方式
        /// </summary>
        public int? DeliveryId { get; set; }

        /// <summary>
        /// Delivery instruction
        /// 发货指示
        /// </summary>
        public string? DeliveryInstruction { get; init; }

        /// <summary>
        /// Start date
        /// 开始时间
        /// </summary>
        public DateTimeOffset? StartDate { get; init; }

        /// <summary>
        /// End date
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndDate { get; init; }

        /// <summary>
        /// Delivery address id
        /// 发货地址编号
        /// </summary>
        public int? AddressId { get; init; }

        /// <summary>
        /// Contact id
        /// 联系人编号
        /// </summary>
        public long? ContactId { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Tax amount
        /// 纳税金额
        /// </summary>
        public decimal? TaxAmount { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public JsonDocument? Data { get; set; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        public IEnumerable<string>? Tags { get; init; }

        /// <summary>
        /// Promotions
        /// 促销
        /// </summary>
        public IEnumerable<PromotionSaleItemBase>? Promotions { get; init; }

        /// <summary>
        /// Lines
        /// 行
        /// </summary>
        public required IEnumerable<OrderLineRQ> Lines { get; init; }

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
            if (Source != null && Source.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Source));
            }

            if (SourceId != null && SourceId.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(SourceId));
            }

            if (!new CurrencyAttribute().IsValid(Currency))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Currency));
            }

            if (!new LanguageCodeAttribute().IsValid(Culture))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Culture));
            }

            if (Title != null && Title.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 1280))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (PaymentInstruction != null && PaymentInstruction.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(PaymentInstruction));
            }

            if (DeliveryInstruction != null && DeliveryInstruction.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(DeliveryInstruction));
            }

            if (AssignedId != null && AssignedId.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            if (Lines.Count() > 1000)
            {
                return ApplicationErrors.TooManyItems.AsResult(nameof(Lines));
            }

            foreach (var line in Lines)
            {
                if (line.Validate()?.Ok is false)
                {
                    var result = ApplicationErrors.NoValidData.AsResult(nameof(Lines));
                    result.Detail = line.Title;
                    return result;
                }
            }

            return null;
        }
    }
}
