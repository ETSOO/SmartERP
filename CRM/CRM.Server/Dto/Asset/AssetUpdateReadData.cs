using com.etsoo.CoreFramework.Business;
using PlatformShared.Database.Models;
using System.Text.Json;

namespace CRM.Server.Dto.Asset
{
    /// <summary>
    /// Asset update read data
    /// 更新资产读取数据
    /// </summary>
    public class AssetUpdateReadData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Person (owner) id
        /// 所有者编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long? SupplierId { get; init; }

        /// <summary>
        /// Serial number
        /// 序列号
        /// </summary>
        public required string Sn { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTimeOffset Expiry { get; init; }

        /// <summary>
        /// Expiry check
        /// 到期检查
        /// </summary>
        public bool? ExpiryCheck { get; init; }

        /// <summary>
        /// Remaining times
        /// 剩余次数
        /// </summary>
        public int? Times { get; init; }

        /// <summary>
        /// Remaining amount
        /// 剩余金额
        /// </summary>
        public decimal? Amount { get; init; }

        /// <summary>
        /// Sensitive data
        /// 敏感数据
        /// </summary>
        public string? SensitiveData { get; init; }

        /// <summary>
        /// Health check URL
        /// 健康检查网址
        /// </summary>
        public string? HealthCheckUrl { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public PersonAssetData? Data { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }
    }
}
