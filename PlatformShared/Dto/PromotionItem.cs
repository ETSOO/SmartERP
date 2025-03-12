namespace PlatformShared.Dto
{
    /// <summary>
    /// Promotion item
    /// 促销项
    /// </summary>
    public record PromotionItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Promotion code
        /// 促销码
        /// </summary>
        public PromotionCode Code { get; set; } = default!;

        /// <summary>
        /// Minimum spending amount
        /// 最低消费金额
        /// </summary>
        public decimal MinAmount { get; set; }

        /// <summary>
        /// Discount percentage, like 10 for 10%
        /// 折扣百分比，如 10 代表 10%
        /// </summary>
        public int Discount { get; set; }

        /// <summary>
        /// Whether the promotion is stackable
        /// 优惠是否可叠加
        /// </summary>
        public bool Stackable { get; set; }
    }
}
