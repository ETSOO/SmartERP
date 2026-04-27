using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Json;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.Dto.Product;
using PlatformShared.Database.Models;
using System.Text.Json;

namespace CRM.Server.RQ.Product
{
    /// <summary>
    /// Create product request data
    /// 创建产品请求数据
    /// </summary>
    public record ProductCreateRQ : IModelValidator
    {
        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Unit id
        /// 产品单位编号
        /// </summary>
        public int? UnitId { get; init; }

        /// <summary>
        /// Minimum purchase qty
        /// 最少购买量
        /// </summary>
        public decimal? MinQty { get; init; }

        /// <summary>
        /// Purchase minimum unit
        /// 购买最小单位
        /// </summary>
        public decimal? StepQty { get; init; }

        /// <summary>
        /// Maximum purchase qty
        /// 最大购买量
        /// </summary>
        public decimal? CapQty { get; init; }

        /// <summary>
        /// Asset qty
        /// 资产数量
        /// </summary>
        public int? AssetQty { get; init; }

        /// <summary>
        /// Validity
        /// 有效期
        /// </summary>
        public int? Validity { get; init; }

        /// <summary>
        /// Usage
        /// 使用范围
        /// </summary>
        public ProductUsage? Usage { get; init; }

        /// <summary>
        /// Sale scope
        /// 销售范围
        /// </summary>
        public ProductScope? Scope { get; init; }

        /// <summary>
        /// Query keyword
        /// 查询关键词
        /// </summary>
        public string? QueryKeyword { get; init; }

        /// <summary>
        /// Price
        /// 价格
        /// </summary>
        public ProductPriceItem? Price { get; init; }

        /// <summary>
        /// Tax rate
        /// 税率
        /// </summary>
        public decimal? TaxRate { get; init; }

        /// <summary>
        /// Introduction Url
        /// 介绍链接
        /// </summary>
        public string? IntroductionUrl { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<int>? Categories { get; init; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        public IEnumerable<string>? Tags { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public JsonDocument? Data { get; init; }

        /// <summary>
        /// Modifiers
        /// 定制选项
        /// </summary>
        public JsonDocument? Modifiers { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Name.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (AssignedId != null && AssignedId.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 2560))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (MinQty != null && MinQty is not (> 0 and < 9999))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(MinQty));
            }

            if (StepQty != null && StepQty is not (> 0 and < 9999))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(StepQty));
            }

            if (CapQty != null && CapQty is not (> 0 and < 99999999))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(CapQty));
            }

            if (Price != null && !Price.Validate())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Price));
            }

            if (IntroductionUrl != null && IntroductionUrl.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(IntroductionUrl));
            }

            if (QueryKeyword != null && QueryKeyword.Length is not (>= 1 and <= 30))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(QueryKeyword));
            }

            if (Tags != null && Tags.Any(t => t.Length is < 1 or > 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Tags));
            }

            if (Modifiers != null && !CustomFieldSchema.Create().Evaluate(Modifiers.RootElement).IsValid)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Modifiers));
            }

            return null;
        }
    }
}
