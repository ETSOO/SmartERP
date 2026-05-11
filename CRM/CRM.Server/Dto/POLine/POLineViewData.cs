using com.etsoo.CoreFramework.Business;
using PlatformShared.Dto;
using System.Text.Json;

namespace CRM.Server.Dto.POLine
{
    /// <summary>
    /// Purchase order line view data
    /// 采购项目浏览数据
    /// </summary>
    public record POLineViewData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Buyer id
        /// 购买方编号
        /// </summary>
        public long BuyerId { get; init; }

        /// <summary>
        /// PO title
        /// 采购标题
        /// </summary>
        public required string POTitle { get; init; }

        /// <summary>
        /// PO id
        /// 采购编号
        /// </summary>
        public long POId { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

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
        /// Qty delivered
        /// 已交付数量
        /// </summary>
        public decimal? QtyDelivered { get; init; }

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
        /// PO user id
        /// 采购用户编号
        /// </summary>
        public long POUserId { get; init; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long? SupplierId { get; init; }

        /// <summary>
        /// Supplier name
        /// 供应商名称
        /// </summary>
        public string? SupplierName { get; init; }

        /// <summary>
        /// Asset id
        /// 资产编号
        /// </summary>
        public int? AssetId { get; init; }

        /// <summary>
        /// Serial number
        /// 序列号
        /// </summary>
        public string? AssetSn { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// PO status
        /// 采购状态
        /// </summary>
        public EntityStatus POStatus { get; init; }

        /// <summary>
        /// Modifiers
        /// 定制选项
        /// </summary>
        public JsonDocument? Modifiers { get; init; }

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
        /// BOM line id
        /// BOM行编号
        /// </summary>
        public long? BomId { get; init; }

        /// <summary>
        /// Bom line title
        /// BOM行标题
        /// </summary>
        public string? BomTitle { get; init; }

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

        /// <summary>
        /// Whether the order line is restorable
        /// 是否可恢复原状
        /// </summary>
        public bool IsRestorable { get; set; }
    }
}
