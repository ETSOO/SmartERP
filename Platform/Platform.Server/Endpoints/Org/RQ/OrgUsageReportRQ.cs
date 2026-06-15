namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Organization usage report request data
    /// 机构使用报告请求数据
    /// </summary>
    public record OrgUsageReportRQ : IOrgRQ
    {
        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; set; }

        /// <summary>
        /// Year
        /// 年
        /// </summary>
        public int? Year { get; init; }

        /// <summary>
        /// Whether to include last year data
        /// 是否包含去年数据
        /// </summary>
        public bool? HasLastYear { get; init; }
    }
}
