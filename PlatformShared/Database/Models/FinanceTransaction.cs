namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Finance transaction kind
    /// 财务交易类型
    /// </summary>
    public enum FinanceTransactionKind : byte
    {
        /// <summary>
        /// Collect
        /// 收款
        /// </summary>
        Collect = 1,

        /// <summary>
        /// Payment
        /// 付款
        /// </summary>
        Payment = 2,

        /// <summary>
        /// Transfer
        /// 转账
        /// </summary>
        Transfer = 3,

        /// <summary>
        /// Settlement
        /// 月结
        /// </summary>
        Settlement = 9,

        /// <summary>
        /// Adjustment
        /// 手工调整
        /// </summary>
        Adjustment = 77,

        /// <summary>
        /// Init
        /// 初始化
        /// </summary>
        Init = 99
    }

    /// <summary>
    /// Finance transaction
    /// 财务交易
    /// </summary>
    public class FinanceTransaction
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FinanceTransactionKind Kind { get; set; }

        /// <summary>
        /// Account id
        /// 账号编号
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Order id related
        /// 关联的订单/采购编号
        /// </summary>
        public long? OrderId { get; set; }

        /// <summary>
        /// Reference id
        /// 参考编号
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Target person id
        /// 目标人员编号
        /// </summary>
        public long? TargetPersonId { get; set; }

        /// <summary>
        /// Target account id
        /// 目标账户编号
        /// </summary>
        public int? TargetAccountId { get; set; }

        /// <summary>
        /// Author id
        /// 作者编号
        /// </summary>
        public long AuthorId { get; set; }
        
        /// <summary>
        /// Creation time
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Times
        /// 次数
        /// </summary>
        public int? Times { get; set; }

        /// <summary>
        /// Inner reference id
        /// 内部参考编号
        /// </summary>
        public Guid? InnerRef { get; set; }

        /// <summary>
        /// Offset id
        /// 冲减编号
        /// </summary>
        public long? OffsetId { get; set; }

        /// <summary>
        /// Account
        /// 账户
        /// </summary>
        public FinanceAccount Account { get; set; } = default!;

        /// <summary>
        /// Author
        /// 作者
        /// </summary>
        public Person Author { get; set; } = default!;

        /// <summary>
        /// Order
        /// 订单
        /// </summary>
        public OrderHeader? Order { get; set; }

        /// <summary>
        /// Target account
        /// 目标账户
        /// </summary>
        public FinanceAccount? TargetAccount { get; set; }

        /// <summary>
        /// Target person
        /// 目标人员
        /// </summary>
        public Person? TargetPerson { get; set; }
    }
}
