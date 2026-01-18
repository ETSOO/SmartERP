using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.Promotion
{
    /// <summary>
    /// Promotion query data
    /// 促销查询数据
    /// </summary>
    public record PromotionQueryData
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
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Minimum spending amount
        /// 最低消费金额
        /// </summary>
        public decimal MinAmount { get; init; }

        /// <summary>
        /// Discount percentage, like 10 for 10%
        /// 折扣百分比，如 10 代表 10%
        /// </summary>
        public int Discount { get; init; }

        /// <summary>
        /// Number of coupons
        /// 优惠券数量
        /// </summary>
        public int? Coupons { get; init; }

        /// <summary>
        /// Number of coupons applied
        /// 已使用优惠券数量
        /// </summary>
        public int CouponsApplied { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
