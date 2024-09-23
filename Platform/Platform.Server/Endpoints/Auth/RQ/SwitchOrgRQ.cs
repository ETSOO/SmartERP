namespace Platform.Server.Endpoints.Auth.RQ
{
    /// <summary>
    /// Switch organization request data
    /// 切换机构请求数据
    /// </summary>
    public record SwitchOrgRQ
    {
        /// <summary>
        /// Target organization id
        /// 目标机构编号
        /// </summary>
        public required int OrganizationId { get; init; }

        /// <summary>
        /// From organization id
        /// 来源机构编号
        /// </summary>
        public int? FromOrganizationId { get; init; }
    }
}
