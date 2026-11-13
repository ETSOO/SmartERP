using PlatformShared.Database.Models;

namespace CRM.Server.Dto.FinanceTransaction
{
    /// <summary>
    /// Finance transaction query data
    /// 财务交易查询数据
    /// </summary>
    public record FinanceTransactionQueryData
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
        /// Reference id
        /// 参考编号
        /// </summary>
        public string? ReferenceId { get; init; }

        /// <summary>
        /// Order id related
        /// 关联的订单/采购编号
        /// </summary>
        public long? OrderId { get; init; }

        /// <summary>
        /// Order kind
        /// 订单类型
        /// </summary>
        public OrderKind? OrderKind { get; set; }

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

        /// <summary>
        /// Author name
        /// 作者姓名
        /// </summary>
        public required string AuthorName { get; init; }
    }
}
