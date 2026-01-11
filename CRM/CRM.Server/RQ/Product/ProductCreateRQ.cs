using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Product;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Product
{
    /// <summary>
    /// Create product request data
    /// 创建产品请求数据
    /// </summary>
    public record ProductCreateRQ
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
        /// Foreign name
        /// 外文名称
        /// </summary>
        public string? ForeignName { get; init; }

        /// <summary>
        /// Foreign description
        /// 外文描述
        /// </summary>
        public string? ForeignDescription { get; init; }

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
        /// Inventory management way
        /// 库存管理方式
        /// </summary>
        public ProductInventoryWay? InventoryWay { get; init; }

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

            if (ForeignName != null && ForeignName.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(ForeignName));
            }

            if (ForeignDescription != null && ForeignDescription.Length is not (>= 1 and <= 2560))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(ForeignDescription));
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

            if (QueryKeyword != null && QueryKeyword.Length is not (>= 1 and <= 30))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(QueryKeyword));
            }

            if (Tags != null && Tags.Any(t => t.Length is < 1 or > 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Tags));
            }

            return null;
        }
    }
}
