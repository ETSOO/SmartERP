namespace PlatformShared.Dto
{
    /// <summary>
    /// Promotion sale base item
    /// 销售促销基础项目
    /// </summary>
    public record PromotionSaleItemBase
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }
    }

    /// <summary>
    /// Promotion sale item
    /// 销售促销项目
    /// </summary>
    public record PromotionSaleItem : PromotionSaleItemBase
    {
        /// <summary>
        /// Times
        /// 次数
        /// </summary>
        public int Times { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }
    }
}
