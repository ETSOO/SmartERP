using com.etsoo.CoreFramework.Business;
using PlatformShared.Dto;
using System.Text.Json;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Order line
    /// 订单行
    /// </summary>
    public class OrderLine
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; set; }

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
        /// Original price
        /// 原价
        /// </summary>
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// Cost price
        /// 成本价
        /// </summary>
        public decimal CostPrice { get; set; }

        /// <summary>
        /// Sale price
        /// 销售价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// Asset qty
        /// 资产数量
        /// </summary>
        public int AssetQty { get; set; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Discount
        /// 折扣
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Promotions
        /// 促销细节
        /// </summary>
        public IEnumerable<PromotionSaleItem>? Promotions { get; set; }

        /// <summary>
        /// Start time
        /// 开始时间
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// End time
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// Asset id
        /// 关联的资产编号
        /// </summary>
        public int? AssetId { get; set; }

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
        /// User id
        /// 执行用户编号
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long? SupplierId { get; set; }

        /// <summary>
        /// Asset
        /// 资产
        /// </summary>
        public PersonAsset? Asset { get; set; }

        /// <summary>
        /// Order
        /// 订单
        /// </summary>
        public OrderHeader Order { get; set; } = default!;

        /// <summary>
        /// Product
        /// 产品
        /// </summary>
        public Product Product { get; set; } = default!;

        /// <summary>
        /// Supplier
        /// 供应商
        /// </summary>
        public Person? Supplier { get; set; }

        /// <summary>
        /// User
        /// 执行用户
        /// </summary>
        public virtual Person? User { get; set; }
    }
}
