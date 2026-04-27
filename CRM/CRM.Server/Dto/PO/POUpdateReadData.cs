using com.etsoo.CoreFramework.Business;
using System.Text.Json;

namespace CRM.Server.Dto.PO
{
    /// <summary>
    /// PO update read data
    /// 更新订单读取数据
    /// </summary>
    public record POUpdateReadData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Source
        /// 来源
        /// </summary>
        public string? Source { get; init; }

        /// <summary>
        /// Source id
        /// 来源编号
        /// </summary>
        public string? SourceId { get; init; }

        /// <summary>
        /// Customer id
        /// 客户编号
        /// </summary>
        public required long CustomerId { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public required string Culture { get; init; }

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
        /// Payment way
        /// 付款方式
        /// </summary>
        public int? PaymentId { get; set; }

        /// <summary>
        /// Payment instruction
        /// 付款指示
        /// </summary>
        public string? PaymentInstruction { get; init; }

        /// <summary>
        /// Delivery way
        /// 发货方式
        /// </summary>
        public int? DeliveryId { get; set; }

        /// <summary>
        /// Delivery instruction
        /// 发货指示
        /// </summary>
        public string? DeliveryInstruction { get; init; }

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
        /// Delivery address id, targeting delivery address may be changed
        /// 发货地址编号，目标发货地址可能会改变
        /// </summary>
        public int? AddressId { get; init; }

        /// <summary>
        /// Contact id
        /// 联系人编号
        /// </summary>
        public long? ContactId { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Tax amount
        /// 纳税金额
        /// </summary>
        public decimal? TaxAmount { get; init; }

        /// <summary>
        /// Amount
        /// 总金额
        /// </summary>
        public decimal Amount { get; init; }

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
        /// JSON data
        /// JSON 数据
        /// </summary>
        public JsonDocument? Data { get; set; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        public IEnumerable<string>? Tags { get; init; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long UserId { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }
    }
}

