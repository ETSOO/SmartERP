using com.etsoo.CoreFramework.Business;

namespace Admin.Server.RQ.Query
{
    /// <summary>
    /// All app request data
    /// 所有应用请求数据
    /// </summary>
    public record AllAppRQ : QueryIntRQ
    {
        /// <summary>
        /// Identity type
        /// 身份类型
        /// </summary>
        public IdentityType? IdentityType { get; init; }

        /// <summary>
        /// App ID
        /// 应用编号
        /// </summary>
        public int? AppId { get; init; }

        /// <summary>
        /// Organization ID
        /// 机构编号
        /// </summary>
        public int? OrgId { get; init; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; set; }

        /// <summary>
        /// Expiry days
        /// 到期天数
        /// </summary>
        public short? ExpiryDays { get; set; }

        /// <summary>
        /// Creation start
        /// 登记开始时间
        /// </summary>
        public DateTime? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 登记结束时间
        /// </summary>
        public DateTime? CreationEnd { get; init; }
    }
}
