using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Dto.Document.Asset
{
    /// <summary>
    /// Asset view data
    /// 资产视图数据
    /// </summary>
    public record AssetViewData
    {
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
        /// Remaining times
        /// 剩余次数
        /// </summary>
        public int Times { get; init; }

        /// <summary>
        /// Remaining amount
        /// 剩余金额
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTimeOffset Expiry { get; init; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int OrgId { get; init; }
    }
}
