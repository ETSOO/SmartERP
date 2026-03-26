namespace PlatformShared.Dto
{
    /// <summary>
    /// Promotion code line
    /// 促销码行数据
    /// </summary>
    public record PromotionCodeLine : IPromotionCodeLine
    {
        /// <summary>
        /// Price
        /// 成交价格
        /// </summary>
        public decimal Price { get; init; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Current price
        /// 当前价格
        /// </summary>
        public decimal? CurrentPrice { get; set; }
    }
}
