using com.etsoo.CoreFramework.Business;
using PlatformShared.Dto;
using System.Text.Json;

namespace CRM.Server.Dto.OrderLine
{
    /// <summary>
    /// Order line view data
    /// 订单项目浏览数据
    /// </summary>
    public record OrderLineViewData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Order title
        /// 订单标题
        /// </summary>
        public required string OrderTitle { get; init; }

        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long OrderId { get; init; }

        /// <summary>
        /// Product name
        /// 产品名称
        /// </summary>
        public required string ProductName { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

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
        /// Original price
        /// 原价
        /// </summary>
        public decimal OriginalPrice { get; init; }

        /// <summary>
        /// Cost price
        /// 成本价
        /// </summary>
        public decimal CostPrice { get; init; }

        /// <summary>
        /// Price
        /// 价格
        /// </summary>
        public decimal Price { get; init; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Asset qty
        /// 资产数量
        /// </summary>
        public decimal AssetQty { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Discount
        /// 折扣
        /// </summary>
        public decimal Discount { get; init; }

        /// <summary>
        /// Promotions
        /// 促销细节
        /// </summary>
        public IEnumerable<PromotionSaleItem>? Promotions { get; init; }

        /// <summary>
        /// Start time
        /// 开始时间
        /// </summary>
        public DateTimeOffset? StartTime { get; init; }

        /// <summary>
        /// End time
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndTime { get; init; }

        /// <summary>
        /// User name
        /// 用户姓名
        /// </summary>
        public string? UserName { get; init; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long? UserId { get; init; }

        /// <summary>
        /// Order user id
        /// 订单用户编号
        /// </summary>
        public long OrderUserId { get; init; }

        /// <summary>
        /// Asset id
        /// 资产编号
        /// </summary>
        public int? AssetId { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Order status
        /// 订单状态
        /// </summary>
        public EntityStatus OrderStatus { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public JsonDocument? Data { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }

        /// <summary>
        /// Whether the order line is startable
        /// 是否可开始执行
        /// </summary>
        public bool IsStartable { get; set; }

        /// <summary>
        /// Whether the order line is completable
        /// 是否可完成执行
        /// </summary>
        public bool IsCompletable { get; set; }
    }
}
