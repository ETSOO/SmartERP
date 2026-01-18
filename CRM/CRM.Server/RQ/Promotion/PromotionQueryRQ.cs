namespace CRM.Server.RQ.Promotion
{
    /// <summary>
    /// Promotion query request data
    /// 促销查询请求数据
    /// </summary>
    public record PromotionQueryRQ : PromotionListRQ
    {
        /// <summary>
        /// Coupons applied start
        /// 优惠券使用量起使
        /// </summary>
        public int? CouponsAppliedStart { get; init; }

        /// <summary>
        /// Coupons applied end
        /// 优惠卷使用量结束
        /// </summary>
        public int? CouponsAppliedEnd { get; init; }
    }
}
