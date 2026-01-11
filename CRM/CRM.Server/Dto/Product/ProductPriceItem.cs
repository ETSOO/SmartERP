namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Product price item
    /// 产品价格项
    /// </summary>
    public record ProductPriceItem
    {
        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Retail price
        /// 零售价
        /// </summary>
        public required decimal RetailPrice { get; init; }

        /// <summary>
        /// Promotion price
        /// 促销价
        /// </summary>
        public decimal? PromotionPrice { get; init; }

        /// <summary>
        /// Channel price
        /// 渠道价
        /// </summary>
        public decimal? ChannelPrice { get; init; }

        /// <summary>
        /// Cost price
        /// 成本价
        /// </summary>
        public decimal? CostPrice { get; init; }
    }
}
