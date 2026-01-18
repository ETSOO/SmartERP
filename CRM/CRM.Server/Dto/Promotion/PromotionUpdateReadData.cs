using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.Promotion
{
    /// <summary>
    /// Promotion update read data
    /// 更新促销读取数据
    /// </summary>
    public record PromotionUpdateReadData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Code
        /// 代码
        /// </summary>
        public short Code { get; init; }

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
        /// Valid start date
        /// 有效开始时间
        /// </summary>
        public DateTimeOffset ValidStart { get; init; }

        /// <summary>
        /// Valid start end
        /// 有效结束时间
        /// </summary>
        public DateTimeOffset ValidEnd { get; init; }

        /// <summary>
        /// Max coupons
        /// 最大优惠券
        /// </summary>
        public int? Coupons { get; init; }

        /// <summary>
        /// Stackable
        /// 促销是否可叠加
        /// </summary>
        public bool? Stackable { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }
    }
}
