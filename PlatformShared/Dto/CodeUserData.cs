namespace PlatformShared.Dto
{
    /// <summary>
    /// Code user data
    /// 验证码用户数据
    /// </summary>
    public record CodeUserData
    {
        /// <summary>
        /// Name
        /// 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Given name
        /// 名
        /// </summary>
        public string? GivenName { get; init; }

        /// <summary>
        /// Family name
        /// 姓
        /// </summary>
        public string? FamilyName { get; init; }

        /// <summary>
        /// Current organization id
        /// 当前机构编号
        /// </summary>
        public required int OrganizationId { get; init; }

        /// <summary>
        /// Organization name
        /// 机构名称
        /// </summary>
        public required string OrganizationName { get; init; }
    }
}
