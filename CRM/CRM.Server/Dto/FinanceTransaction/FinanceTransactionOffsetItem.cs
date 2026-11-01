using PlatformShared.Database.Models;

namespace CRM.Server.Dto.FinanceTransaction
{
    /// <summary>
    /// Finance transaction offset item
    /// 财务交易冲销项目
    /// </summary>
    public record FinanceTransactionOffsetItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FinanceTransactionKind Kind { get; init; }

        /// <summary>
        /// Account id
        /// 账号编号
        /// </summary>
        public int AccountId { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Order id related
        /// 关联的订单/采购编号
        /// </summary>
        public long? OrderId { get; init; }

        /// <summary>
        /// Reference id
        /// 参考编号
        /// </summary>
        public string? ReferenceId { get; init; }

        /// <summary>
        /// Inner reference id
        /// 内部参考编号
        /// </summary>
        public Guid? InnerRef { get; init; }
    }
}
