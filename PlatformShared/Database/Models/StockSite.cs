namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Stock site summary
    /// 库存点汇总
    /// </summary>
    public class StockSite
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Qty.
        /// 数量
        /// </summary>
        public decimal? Qty { get; set; }

        /// <summary>
        /// Last refresh time
        /// 上次刷新时间
        /// </summary>
        public DateTime RefreshTime { get; set; }

        /// <summary>
        /// Location
        /// 位置
        /// </summary>
        public PersonAddress? Location { get; set; }

        /// <summary>
        /// Product
        /// 产品
        /// </summary>
        public Product Product { get; set; } = default!;
    }
}
