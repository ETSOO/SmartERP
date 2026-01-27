using com.etsoo.CoreFramework.Models;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.System
{
    /// <summary>
    /// Update settings request data
    /// 更新设置请求数据
    /// </summary>
    public record UpdateSettingsRQ : UpdateModel
    {
        /// <summary>
        /// Main customer type
        /// 主要的客户类型
        /// </summary>
        public CustomerType? MainCustomerType { get; init; }

        /// <summary>
        /// Currencies
        /// 币种
        /// </summary>
        public IEnumerable<string>? Currencies { get; init; }

        /// <summary>
        /// Supplier currencies
        /// 供应商币种
        /// </summary>
        public IEnumerable<string>? SupplierCurrencies { get; init; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public IEnumerable<string>? Cultures { get; init; }

        /// <summary>
        /// Whether has inventory management
        /// 是否有库存管理
        /// </summary>
        public bool? HasInventory { get; init; }

        /// <summary>
        /// Default tax rate
        /// 默认税率
        /// </summary>
        public decimal? TaxRate { get; init; }
    }
}
