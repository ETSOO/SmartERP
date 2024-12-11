using com.etsoo.CoreFramework.Business;

namespace Platform.Server.Dto.App
{
    /// <summary>
    /// Application purchased query data
    /// 购买的应用查询数据
    /// </summary>
    public record AppPurchasedQueryData : AppQueryData
    {
        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; init; }

        /// <summary>
        /// Expiry days
        /// 到期天数
        /// </summary>
        public int? ExpiryDays { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
