using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person asset
    /// 个人资产
    /// </summary>
    public class PersonAsset
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int OrgId { get; set; }

        /// <summary>
        /// Person id
        /// 所有者编号
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long? SupplierId { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Serial number
        /// 序列号
        /// </summary>
        public string Sn { get; set; } = default!;

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTimeOffset Expiry { get; set; }

        /// <summary>
        /// Remaining times
        /// 剩余次数
        /// </summary>
        public short? Times { get; set; }

        /// <summary>
        /// Remaining amount
        /// 剩余金额
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Quantity
        /// 数量
        /// </summary>
        public short Qty { get; set; }

        /// <summary>
        /// Sensitive data
        /// 敏感数据
        /// </summary>
        public string? SensitiveData { get; set; }

        /// <summary>
        /// Operator's core user id
        /// 操作员的核心用户编号
        /// </summary>
        public int CoreUserId { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; }

        /// <summary>
        /// Creation time
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Health check URL
        /// 健康检查网址
        /// </summary>
        public string? HealthCheckUrl { get; set; }

        /// <summary>
        /// Health check schedule
        /// 健康检查计划
        /// </summary>
        public DateTimeOffset? HealthCheckSchedule { get; set; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; set; }
    }
}
