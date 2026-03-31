namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Order payment kind
    /// 订单支付方式类型
    /// </summary>
    public enum OrderPaymentKind : byte
    {
        /// <summary>
        /// Cash
        /// 现金
        /// </summary>
        Cash = 1,

        /// <summary>
        /// Credit card
        /// 信用卡
        /// </summary>
        CreditCard = 2,

        /// <summary>
        /// Bank transfer
        /// 银行转账
        /// </summary>
        BankTransfer = 3,

        /// <summary>
        /// Check
        /// 支票
        /// </summary>
        Check = 4,

        /// <summary>
        /// Alipay
        /// 支付宝
        /// </summary>
        Alipay = 10,

        /// <summary>
        /// WeChat Pay
        /// 微信支付
        /// </summary>
        WeChatPay = 11,

        /// <summary>
        /// Other
        /// 其他
        /// </summary>
        Other = 255
    }

    /// <summary>
    /// Order payment method
    /// 订单支付方式
    /// </summary>
    public class OrderPayment
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public OrderPaymentKind Kind { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Is valid
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public short OrderIndex { get; set; }

        /// <summary>
        /// Orders
        /// 订单
        /// </summary>
        public ICollection<OrderHeader> Orders { get; set; } = default!;
    }
}
