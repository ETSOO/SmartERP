using PlatformShared.Dto;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Query product data for sale
    /// 查询产品数据用于销售
    /// </summary>
    public record QueryForSaleData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Customer name
        /// 客户端名称
        /// </summary>
        public string? CustomerName { get; set; }

        /// <summary>
        /// Customer description
        /// 客户端描述
        /// </summary>
        public string? CustomerDescription { get; set; }

        /// <summary>
        /// Customer assigned id
        /// 客户分配的编号
        /// </summary>
        public string? CustomerAssignedId { get; set; }

        /// <summary>
        /// Customer retail price
        /// 客户端零售价
        /// </summary>
        public decimal? CustomerRetailPrice { get; set; }

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
        public short? AssetQty { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Retail price
        /// 零售价
        /// </summary>
        public decimal RetailPrice { get; init; }

        /// <summary>
        /// Promotion price
        /// 促销价
        /// </summary>
        public decimal? PromotionPrice { get; init; }

        /// <summary>
        /// Cost price
        /// 成本价
        /// </summary>
        [JsonIgnore]
        public decimal? CostPrice { get; init; }

        /// <summary>
        /// Unit id
        /// 单位编号
        /// </summary>
        [JsonIgnore]
        public int UnitId { get; init; }

        /// <summary>
        /// Unit name
        /// 单位名称
        /// </summary>
        public required string UnitName { get; set; }

        /// <summary>
        /// Modifiers
        /// 定制选项
        /// </summary>
        public JsonDocument? Modifiers { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        [JsonIgnore]
        public IEnumerable<int>? CategoryIds { get; init; }

        /// <summary>
        /// Categories all
        /// 所有类目
        /// </summary>
        [JsonIgnore]
        public IEnumerable<int>? CategoryIdsAll { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<CategoryItemWithParents>? Categories { get; set; }

        /// <summary>
        /// Promotions
        /// 促销
        /// </summary>
        public IEnumerable<PromotionItem> Promotions { get; set; } = [];
    }
}
