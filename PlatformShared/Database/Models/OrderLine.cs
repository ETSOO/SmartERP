using com.etsoo.CoreFramework.Business;
using PlatformShared.Dto;

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
        /// Foreign title
        /// 外文标题
        /// </summary>
        public string? ForeignTitle { get; set; }

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
        public short AssetQty { get; set; }

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
        public IEnumerable<PromotionItem>? Promotions { get; set; }

        /// <summary>
        /// Start time
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// End time
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

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
        public string? Data { get; set; }
    }
}
