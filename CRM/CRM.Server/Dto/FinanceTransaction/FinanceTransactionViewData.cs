using PlatformShared.Database.Models;

namespace CRM.Server.Dto.FinanceTransaction
{
    /// <summary>
    /// Finance transaction view reference item data
    /// 财务交易浏览引用项目数据
    /// </summary>
    public record FinanceTransactionViewDataRefItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Account kind
        /// 账户类型
        /// </summary>
        public FinanceAccountKind AccountKind { get; init; }

        /// <summary>
        /// Bank
        /// 开户银行
        /// </summary>
        public required string Bank { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }
    }

    /// <summary>
    /// Finance transaction view data
    /// 财务交易浏览数据
    /// </summary>
    public record FinanceTransactionViewData
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
        /// Account
        /// 账户
        /// </summary>
        public required string Account { get; init; }

        /// <summary>
        /// Account id
        /// 账号编号
        /// </summary>
        public int AccountId { get; init; }

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
        public OrderKind? OrderKind { get; init; }

        /// <summary>
        /// Order title
        /// 订单标题
        /// </summary>
        public string? OrderTitle { get; init; }

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
        /// Author id
        /// 作者编号
        /// </summary>
        public long AuthorId { get; init; }

        /// <summary>
        /// Author name
        /// 作者姓名
        /// </summary>
        public required string AuthorName { get; init; }

        /// <summary>
        /// Offset id
        /// 冲减编号
        /// </summary>
        public long? OffsetId { get; init; }

        /// <summary>
        /// Offset source id
        /// 冲减源编号
        /// </summary>
        public long? OffsetSourceId { get; init; }

        /// <summary>
        /// Inner referred items
        /// 内部引用的项目
        /// </summary>
        public FinanceTransactionViewDataRefItem[]? InnerRefs { get; init; }
    }
}
