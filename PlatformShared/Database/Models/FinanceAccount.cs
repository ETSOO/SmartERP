using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Finance account kind
    /// 财务账户类型
    /// </summary>
    public enum FinanceAccountKind : byte
    {
        /// <summary>
        /// None
        /// 无
        /// </summary>
        None = 0,

        /// <summary>
        /// Cash account
        /// 现金账户
        /// </summary>
        Cash = 1,

        /// <summary>
        /// Transfer account
        /// 转账账户
        /// </summary>
        Transfer = 2,

        /// <summary>
        /// Cash and transfer account
        /// 现金和转账账户
        /// </summary>
        CashAndTransfer = 3,

        /// <summary>
        /// Prepaid
        /// 储值卡
        /// </summary>
        Prepaid = 11,

        /// <summary>
        /// Pass
        /// 次卡
        /// </summary>
        Pass = 12
    }

    /// <summary>
    /// Finance account
    /// 财务账户
    /// </summary>
    public class FinanceAccount
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Owern person Id
        /// 所有人人员编号
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FinanceAccountKind Kind { get; set; }

        /// <summary>
        /// Bank name
        /// 银行名称
        /// </summary>
        public string Bank { get; set; } = default!;

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string Currency { get; set; } = default!;

        /// <summary>
        /// Account number
        /// 账号
        /// </summary>
        public string AccountNumber { get; set; } = default!;

        /// <summary>
        /// SWIFT data
        /// SWIFT 数据
        /// </summary>
        public string? Swift { get; set; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Balance
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Expiry
        /// 过期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; set; }

        /// <summary>
        /// Times
        /// 次数
        /// </summary>
        public short? Times { get; set; }

        /// <summary>
        /// Owner
        /// 所有者
        /// </summary>
        public Person Person { get; set; } = default!;

        /// <summary>
        /// Transactions
        /// 交易
        /// </summary>
        public ICollection<FinanceTransaction> Transactions { get; set; } = default!;

        /// <summary>
        /// Target transactions
        /// 目标交易
        /// </summary>
        public ICollection<FinanceTransaction> TargetTransactions { get; set; } = default!;
    }
}
