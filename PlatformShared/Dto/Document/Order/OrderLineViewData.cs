using com.etsoo.CoreFramework.Business;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlatformShared.Dto.Document.Order
{
    /// <summary>
    /// Order line view data
    /// 订单项目视图数据
    /// </summary>
    public record OrderLineViewData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

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
        /// Product assigned it
        /// 产品分配的编号
        /// </summary>
        public string? ProductAssignedId { get; init; }

        /// <summary>
        /// Product description
        /// 产品描述
        /// </summary>
        public string? ProductDescription { get; init; }

        /// <summary>
        /// Product logo
        /// 产品图标
        /// </summary>
        public string? ProductLogo { get; init; }

        /// <summary>
        /// Unit name
        /// 单位名称
        /// </summary>
        public required string UnitName { get; init; }

        /// <summary>
        /// Base unit
        /// 基础单位
        /// </summary>
        public ProductUnit BaseUnit { get; set; }

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
        public decimal QtyDelivered { get; init; }

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
        public required PromotionSaleItem[] Promotions { get; init; }

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
    }
}
