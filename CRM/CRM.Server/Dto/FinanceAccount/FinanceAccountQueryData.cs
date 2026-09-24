using com.etsoo.CoreFramework.Business;
using PlatformShared.Database.Models;

namespace CRM.Server.Dto.FinanceAccount
{
    /// <summary>
    /// Finance account query data
    /// 财务账户查询数据
    /// </summary>
    public record FinanceAccountQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Owern person Id
        /// 所有人人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Owner person name
        /// 所有者名称
        /// </summary>
        public required string PersonName { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FinanceAccountKind Kind { get; init; }

        /// <summary>
        /// Bank name
        /// 银行名称
        /// </summary>
        public required string Bank { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Account number
        /// 账号
        /// </summary>
        public required string AccountNumber { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Expiry
        /// 过期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; init; }
    }
}
