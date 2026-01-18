namespace PlatformShared.Dto
{
    /// <summary>
    /// Promotion sale item
    /// 销售促销项目
    /// </summary>
    public record PromotionSaleItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }
    }
}
