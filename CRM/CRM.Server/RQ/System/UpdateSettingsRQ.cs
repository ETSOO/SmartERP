using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using Json.Schema;
using PlatformShared.Database.Models;
using System.Text.RegularExpressions;

namespace CRM.Server.RQ.System
{
    /// <summary>
    /// Update settings request data
    /// 更新设置请求数据
    /// </summary>
    public record UpdateSettingsRQ : UpdateModel, IModelValidator
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

        /// <summary>
        /// Whether order monthly report enabled
        /// 订单月报是否启用
        /// </summary>
        public bool? OrderMonthlyReportEnabled { get; init; }

        /// <summary>
        /// Order daily report start hour, 0-23
        /// 订单日报开始小时
        /// </summary>
        public short? OrderDailyReportHour { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (OrderDailyReportHour.HasValue && OrderDailyReportHour.Value is not (>= 0 and <= 23))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(OrderDailyReportHour));
            }

            return null;
        }
    }
}
