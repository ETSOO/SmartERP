namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Organization query resource request data
    /// 机构查询资源请求数据
    /// </summary>
    public record OrgQueryResourceRQ : QueryIntRQ, IOrgRQ
    {
        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; set; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public string? Culture { get; init; }
    }
}
