using PlatformShared.Database.Models;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.FinanceAccount
{
    /// <summary>
    /// Finance account list request data
    /// 财务账户列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(FinanceAccountQueryRQ))]
    public record FinanceAccountListRQ : QueryIntRQ
    {
        /// <summary>
        /// Person (owner) id
        /// 人员（所有者）编号
        /// </summary>
        public long? PersonId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FinanceAccountKind? Kind { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string? Currency { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// Product id or default
        /// 产品编号或默认
        /// </summary>
        public int? ProductIdOrDefault { get; init; }
    }
}
