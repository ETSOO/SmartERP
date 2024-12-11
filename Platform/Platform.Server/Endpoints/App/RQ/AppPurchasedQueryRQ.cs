namespace Platform.Server.Endpoints.App.RQ
{
    /// <summary>
    /// App purchased query data
    /// 已购买应用查询数据
    /// </summary>
    public record AppPurchasedQueryRQ : AppListRQ
    {
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
    }
}
