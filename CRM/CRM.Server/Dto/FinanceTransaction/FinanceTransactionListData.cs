using PlatformShared.Database.Models;

namespace CRM.Server.Dto.FinanceTransaction
{
    /// <summary>
    /// Finance transaction list data
    /// 财务交易列表数据
    /// </summary>
    public record FinanceTransactionListData
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
        /// Title
        /// 主题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }

        /// <summary>
        /// Happen date
        /// 发生日期
        /// </summary>
        public DateTimeOffset? HappenDate { get; init; }
    }
}
