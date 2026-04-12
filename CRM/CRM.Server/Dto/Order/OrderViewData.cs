using com.etsoo.CoreFramework.Business;
using PlatformShared.Dto;

namespace CRM.Server.Dto.Order
{
    /// <summary>
    /// Order view data
    /// 订单浏览数据
    /// </summary>
    public record OrderViewData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Source
        /// 订单源
        /// </summary>
        public string? Source { get; init; }

        /// <summary>
        /// Source id
        /// 源编号
        /// </summary>
        public string? SourceId { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Customer id
        /// 客户编号
        /// </summary>
        public long CustomerId { get; init; }

        /// <summary>
        /// Customer name
        /// 客户名称
        /// </summary>
        public required string CustomerName { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Start date
        /// 开始时间
        /// </summary>
        public DateTimeOffset? StartDate { get; init; }

        /// <summary>
        /// End date
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndDate { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Amount
        /// 总金额
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Paid amount
        /// 已付款金额
        /// </summary>
        public decimal PaidAmount { get; init; }

        /// <summary>
        /// Discount amount
        /// 折扣金额
        /// </summary>
        public decimal Discount { get; init; }

        /// <summary>
        /// Line discount amount
        /// 行折扣金额
        /// </summary>
        public decimal LineDiscount { get; init; }

        /// <summary>
        /// Lines
        /// 行数
        /// </summary>
        public short Lines { get; init; }

        /// <summary>
        /// Items
        /// 项目数
        /// </summary>
        public decimal Items { get; init; }

        /// <summary>
        /// Promotions
        /// 促销细节
        /// </summary>
        public IEnumerable<PromotionSaleItem>? Promotions { get; init; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public required string Culture { get; init; }

        /// <summary>
        /// Payment way
        /// 付款方式
        /// </summary>
        public string? Payment { get; init; }

        /// <summary>
        /// Payment instruction
        /// 付款说明
        /// </summary>
        public string? PaymentInstruction { get; init; }

        /// <summary>
        /// Delivery way
        /// 交付方式
        /// </summary>
        public string? Delivery { get; init; }

        /// <summary>
        /// Delivery instruction
        /// 发货说明
        /// </summary>
        public string? DeliveryInstruction { get; init; }

        /// <summary>
        /// Formatted delivery address
        /// 格式化发货地址
        /// </summary>
        public string? AddressFormatted { get; init; }

        /// <summary>
        /// Contact
        /// 联系人
        /// </summary>
        public string? Contact {  get; init; }

        /// <summary>
        /// Contact id
        /// 联系人编号
        /// </summary>
        public long? ContactId { get; init; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long UserId { get; init; }

        /// <summary>
        /// User
        /// 用户
        /// </summary>
        public required string User { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        public IEnumerable<string>? Tags { get; init; }
    }
}
