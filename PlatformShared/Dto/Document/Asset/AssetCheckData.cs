using com.etsoo.CoreFramework.Business;
using PlatformShared.Database.Models;

namespace PlatformShared.Dto.Document.Asset
{
    /// <summary>
    /// Asset check data
    /// 资产检查数据
    /// </summary>
    public record AssetCheckData
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
        /// Entity name
        /// 实体名称
        /// </summary>
        public required string PersonName { get; init; }

        /// <summary>
        /// Is legal person (enterprise)
        /// 是否为法人（企业）
        /// </summary>
        public bool IsLegalPerson { get; init; }

        /// <summary>
        /// Identity type, employee, customer, or supplier
        /// 标识类型，员工、客户或供应商
        /// </summary>
        public IdentityTypeFlags IdentityType { get; init; }

        /// <summary>
        /// Person user ID
        /// 实体用户编号
        /// </summary>
        public long PersonUserId { get; init; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public IEnumerable<string>? Cultures { get; init; }

        /// <summary>
        /// Operator's core user id
        /// 操作员的核心用户编号
        /// </summary>
        public int CoreUserId { get; init; }

        /// <summary>
        /// Product name
        /// 产品名称
        /// </summary>
        public required string ProductName { get; init; }

        /// <summary>
        /// Whether to notify the owner
        /// 是否通知所有者
        /// </summary>
        public bool NoticeOwner { get; init; }

        /// <summary>
        /// Serial number
        /// 序列号
        /// </summary>
        public required string Sn { get; init; }

        /// <summary>
        /// Health check URL
        /// 健康检查网址
        /// </summary>
        public required string HealthCheckUrl { get; set; }

        /// <summary>
        /// Health check message
        /// 健康检查消息
        /// </summary>
        public string? HealthCheckMessage { get; set; }

        /// <summary>
        /// Health check schedule
        /// 健康检查计划
        /// </summary>
        public DateTimeOffset? HealthCheckSchedule { get; set; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public PersonAssetData? Data { get; init; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int OrgId { get; init; }
    }
}
