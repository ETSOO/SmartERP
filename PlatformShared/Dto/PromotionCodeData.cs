namespace PlatformShared.Dto
{
    /// <summary>
    /// Promotion code data
    /// 促销代码数据
    /// </summary>
    public record PromotionCodeData
    {
        /// <summary>
        /// Related product id
        /// 关联的产品编号
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// Related product category id
        /// 关联的产品类目编号
        /// </summary>
        public int? ProductCategoryId { get; set; }

        /// <summary>
        /// Related person (customer) id
        /// 关联的人员（客户）编号
        /// </summary>
        public long? PersonId { get; set; }

        /// <summary>
        /// Related person category id
        /// 关联的人员类目编号
        /// </summary>
        public int? PersonCategoryId { get; set; }

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
