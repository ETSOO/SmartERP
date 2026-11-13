using PlatformShared.Database.Models;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.FinanceTransaction
{
    /// <summary>
    /// Finance transaction list request data
    /// 财务交易列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(FinanceTransactionQueryRQ))]
    public record FinanceTransactionListRQ : QueryLongRQ
    {
        /// <summary>
        /// Person (owner) id
        /// 人员（所有者）编号
        /// </summary>
        public long? PersonId { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string? Currency { get; init; }

        /// <summary>
        /// Account id
        /// 账号
        /// </summary>
        public int? AccountId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FinanceTransactionKind? Kind { get; init; }

        /// <summary>
        /// Kinds
        /// 类型列表
        /// </summary>
        public IEnumerable<FinanceTransactionKind>? Kinds { get; init; }

        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long? OrderId { get; init; }

        /// <summary>
        /// Inner Reference id
        /// 内部引用编号
        /// </summary>
        public Guid? InnerRef { get; init; }

        /// <summary>
        /// Author id
        /// 作者编号
        /// </summary>
        public long? AuthorId { get; init; }
    }
}
