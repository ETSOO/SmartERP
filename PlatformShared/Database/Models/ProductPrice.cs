namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Product price
    /// 产品价格
    /// </summary>
    public class ProductPrice
    {
        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string Currency { get; set; } = default!;

        /// <summary>
        /// Retail price
        /// 零售价
        /// </summary>
        public decimal RetailPrice { get; set; }

        /// <summary>
        /// Promotion price
        /// 促销价
        /// </summary>
        public decimal? PromotionPrice { get; set; }

        /// <summary>
        /// Channel price
        /// 渠道价
        /// </summary>
        public decimal? ChannelPrice { get; set; }

        /// <summary>
        /// Cost price
        /// 成本价
        /// </summary>
        public decimal? CostPrice { get; set; }

        /// <summary>
        /// Creation time
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Product
        /// 所属产品
        /// </summary>
        public Product Product { get; set; } = default!;
    }
}
