using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.Asset
{
    /// <summary>
    /// Asset view data
    /// 资产浏览数据
    /// </summary>
    public record AssetViewData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Identity type, employee, customer, or supplier
        /// 标识类型，员工、客户或供应商
        /// </summary>
        public IdentityTypeFlags PersonIdentityType { get; set; }

        /// <summary>
        /// Person name
        /// 人员名称
        /// </summary>
        public required string PersonName { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }
        
        /// <summary>
        /// Product name
        /// 产品名称
        /// </summary>
        public required string ProductName { get; init; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long? SupplierId { get; init; }

        /// <summary>
        /// Supplier name
        /// 供应商名称
        /// </summary>
        public string? SupplierName { get; init; }

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
        /// Health check schedule
        /// 健康检查计划
        /// </summary>
        public DateTimeOffset? HealthCheckSchedule { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Creation time
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
