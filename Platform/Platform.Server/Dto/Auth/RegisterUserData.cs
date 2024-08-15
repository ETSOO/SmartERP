namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Register user data
    /// 注册用户数据
    /// </summary>
    public record RegisterUserData
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
    }
}