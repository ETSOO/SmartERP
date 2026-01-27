using PlatformShared.Database.Models;

namespace CRM.Server.Dto.System
{
    /// <summary>
    /// System settings
    /// 系统设置
    /// </summary>
    public record SystemSettings
    {
        /// <summary>
        /// Organization person ID
        /// 机构人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Main customer type
        /// 主要的客户类型
        /// </summary>
        public required CustomerType MainCustomerType { get; init; }

        /// <summary>
        /// Currencies
        /// 币种
        /// </summary>
        public required IEnumerable<string> Currencies { get; init; }

        /// <summary>
        /// Supplier currencies
        /// 供应商币种
        /// </summary>
        public required IEnumerable<string> SupplierCurrencies { get; init; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public required IEnumerable<string> Cultures { get; init; }

        /// <summary>
        /// Whether has inventory management
        /// 是否有库存管理
        /// </summary>
        public bool HasInventory { get; init; }

        /// <summary>
        /// Default tax rate
        /// 默认税率
        /// </summary>
        public decimal? TaxRate { get; init; }
    }
}
