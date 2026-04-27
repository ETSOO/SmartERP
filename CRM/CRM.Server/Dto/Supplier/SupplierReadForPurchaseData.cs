using PlatformShared.Dto;
using System.Text.Json.Serialization;

namespace CRM.Server.Dto.Supplier
{
    /// <summary>
    /// Supplier read data for purchase
    /// 供应商读取采购数据
    /// </summary>
    public class SupplierReadForPurchaseData
    {
        /// <summary>
        /// Supplier data
        /// 供应商数据
        /// </summary>
        public SupplierPurchaseData? Supplier { get; set; }
    }

    /// <summary>
    /// Supplier data for purchase
    /// 供应商采购数据
    /// </summary>
    public record SupplierPurchaseData
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
        /// Promotions
        /// 促销
        /// </summary>
        public IEnumerable<PromotionItem> Promotions { get; set; } = [];
    }
}
