namespace PlatformShared.Dto
{
    /// <summary>
    /// Promotion code line interface
    /// 促销码行数据接口
    /// </summary>
    public interface IPromotionCodeLine
    {
        /// <summary>
        /// Price
        /// 成交价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; }

        /// <summary>
        /// Asset qty
        /// 资产数量
        /// </summary>
        public short AssetQty { get; }

        /// <summary>
        /// Current price
        /// 当前价格
        /// </summary>
        public decimal? CurrentPrice { get; set; }

        /// <summary>
        /// Promotions
        /// 促销
        /// </summary>
        public IEnumerable<PromotionSaleItem>? Promotions { get; set; }
    }
}
