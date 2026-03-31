using com.etsoo.CoreFramework.Business;
using PlatformShared.Dto;
using System.Text.Json;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Order payment way
    /// 订单支付方式
    /// </summary>
    public enum OrderPaymentId : byte
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
        /// Other
        /// 其他
        /// </summary>
        Other = 255
    }

    /// <summary>
    /// Order, purchase or transaction
    /// 订单，采购或交易
    /// </summary>
    public class OrderHeader
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Core organization Id
        /// 核心机构（订单所属机构）编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Is order or purchase order
        /// 是订单还是采购
        /// </summary>
        public bool IsOrder { get; init; }

        /// <summary>
        /// Tax amount
        /// 纳税金额
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Approved discount
        /// 授权折扣
        /// </summary>
        public decimal ApprovedDiscount { get; set; }

        /// <summary>
        /// User Id
        /// 用户（订单所属用户）编号
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Source
        /// 来源
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Source id, can also be customer order number (CON) or supplier order number (PO number)
        /// 来源编号，也可以是客户订单号或者供应商订单号
        /// </summary>
        public string? SourceId { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; set; }

        /// <summary>
        /// Seller id
        /// 销售方编号
        /// </summary>
        public long SellerId { get; set; }

        /// <summary>
        /// Buyer id
        /// 购买方编号
        /// </summary>
        public long BuyerId { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Start date
        /// 开始时间
        /// </summary>
        public DateTimeOffset? StartDate { get; set; }

        /// <summary>
        /// End date
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndDate { get; set; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string Currency { get; set; } = default!;

        /// <summary>
        /// Amount
        /// 总金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Paid amount
        /// 已付款金额
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Discount amount
        /// 折扣金额
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Line discount amount
        /// 行折扣金额
        /// </summary>
        public decimal LineDiscount { get; set; }

        /// <summary>
        /// Lines
        /// 行数
        /// </summary>
        public short Lines { get; set; }

        /// <summary>
        /// Items
        /// 项目数
        /// </summary>
        public decimal Items { get; set; }

        /// <summary>
        /// Promotions
        /// 促销细节
        /// </summary>
        public IEnumerable<PromotionSaleItem>? Promotions { get; set; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public string Culture { get; set; } = default!;

        /// <summary>
        /// Payment way
        /// 付款方式
        /// </summary>
        public int? PaymentId { get; set; }

        /// <summary>
        /// Payment instruction
        /// 付款说明
        /// </summary>
        public string? PaymentInstruction { get; set; }

        /// <summary>
        /// Delivery way
        /// 发货方式
        /// </summary>
        public int? DeliveryId { get; set; }

        /// <summary>
        /// Delivery instruction
        /// 发货说明
        /// </summary>
        public string? DeliveryInstruction { get; set; }

        /// <summary>
        /// Delivery address id
        /// 发货地址编号
        /// </summary>
        public int? AddressId { get; set; }

        /// <summary>
        /// Contact id
        /// 联系人编号
        /// </summary>
        public long? ContactId { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public JsonDocument? Data { get; set; }

        /// <summary>
        /// Tags (id)
        /// 标签（编号）
        /// </summary>
        public List<int>? Tags { get; set; }

        /// <summary>
        /// Address
        /// 地址
        /// </summary>
        public PersonAddress? Address { get; set; }

        /// <summary>
        /// Buyer
        /// 购买方
        /// </summary>
        public Person Buyer { get; set; } = null!;

        /// <summary>
        /// Contact
        /// 联系人
        /// </summary>
        public Person? Contact { get; set; }

        /// <summary>
        /// Delivery
        /// 交付方式
        /// </summary>
        public OrderDelivery? Delivery { get; set; }

        /// <summary>
        /// Payment
        /// 付款方式
        /// </summary>
        public OrderPayment? Payment { get; set; }

        /// <summary>
        /// Seller
        /// 销售方
        /// </summary>
        public Person Seller { get; set; } = null!;

        /// <summary>
        /// Order lines
        /// 订单项目
        /// </summary>
        public ICollection<OrderLine> OrderLines { get; set; } = default!;

        /// <summary>
        /// Profiles
        /// 档案
        /// </summary>
        public ICollection<PersonProfile> Profiles { get; } = default!;
    }
}
