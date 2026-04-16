using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.Order
{
    /// <summary>
    /// Order query data
    /// 订单查询数据
    /// </summary>
    public record OrderQueryData
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
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Customer id
        /// 客户编号
        /// </summary>
        public long CustomerId {  get; init; }

        /// <summary>
        /// Customer name
        /// 客户名称
        /// </summary>
        public required string CustomerName { get; init; }

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
        /// Order discount
        /// 订单折扣
        /// </summary>
        public decimal Discount { get; init; }

        /// <summary>
        /// Product line discount
        /// 单品折扣
        /// </summary>
        public decimal LineDiscount { get; init; }

        /// <summary>
        /// Approved discount
        /// 授权折扣
        /// </summary>
        public decimal ApprovedDiscount { get; init; }

        /// <summary>
        /// Tax amount
        /// 税额
        /// </summary>
        public decimal TaxAmount { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Start date
        /// 开始日期
        /// </summary>
        public DateTimeOffset? StartDate { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
