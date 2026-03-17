using PlatformShared.Dto;
using System.Text.Json.Serialization;

namespace CRM.Server.Dto.Customer
{
    /// <summary>
    /// Customer read data for sale
    /// 客户读取销售数据
    /// </summary>
    public record CustomerReadForSaleData
    {
        /// <summary>
        /// Customer data
        /// 客户数据
        /// </summary>
        public CustomerSaleData? Customer { get; set; }

        /// <summary>
        /// Promotions
        /// 促销
        /// </summary>
        public IEnumerable<PromotionItem> Promotions { get; set; } = [];
    }

    /// <summary>
    /// Customer data for sale
    /// 客户销售数据
    /// </summary>
    public record CustomerSaleData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Preferred name
        /// 首选名
        /// </summary>
        public string? PreferredName { get; init; }

        /// <summary>
        /// Is legal person
        /// 是否法人
        /// </summary>
        public bool IsLegalPerson { get; init; }

        /// <summary>
        /// Categories all
        /// 所有类目
        /// </summary>
        [JsonIgnore]
        public IEnumerable<int>? CategoryIdsAll { get; init; }

        /// <summary>
        /// Promotions
        /// 促销
        /// </summary>
        public IEnumerable<PromotionItem> Promotions { get; set; } = [];
    }
}
