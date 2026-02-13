namespace PlatformShared.Dto
{
    /// <summary>
    /// Promotion code data
    /// 促销代码数据
    /// </summary>
    public record PromotionCodeData
    {
        /// <summary>
        /// Related product ids
        /// 关联的产品编号
        /// </summary>
        public IEnumerable<int>? ProductIds { get; set; }

        /// <summary>
        /// Related exact product category ids
        /// 关联的精确产品类目编号
        /// </summary>
        public IEnumerable<int>? ProductCategoryIds { get; set; }

        /// <summary>
        /// Related person (customer) ids
        /// 关联的人员（客户）编号
        /// </summary>
        public IEnumerable<long>? PersonIds { get; set; }

        /// <summary>
        /// Related exact person category ids
        /// 关联的精确人员类目编号
        /// </summary>
        public IEnumerable<int>? PersonCategoryIds { get; set; }

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
    }
}
