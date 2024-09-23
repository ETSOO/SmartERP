namespace Platform.Server.Dto.Public
{
    /// <summary>
    /// Organization public information
    /// 机构公开信息
    /// </summary>
    public record OrgPublicInfo
    {
        /// <summary>
        /// Organization ID
        /// 机构编号
        /// </summary>
        public int? OrgId { get; init; }

        /// <summary>
        /// Organization name
        /// 机构名称
        /// </summary>
        public string? OrgName { get; init; }

        /// <summary>
        /// Application name
        /// 程序名称
        /// </summary>
        public string? AppName { get; init; }
    }
}
