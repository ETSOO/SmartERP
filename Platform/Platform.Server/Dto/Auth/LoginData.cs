namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Login data
    /// 登录数据
    /// </summary>
    public record LoginData
    {
        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Password
        /// 密码
        /// </summary>
        public required string Password { get; init; }

        /// <summary>
        /// User agent
        /// 用户代理
        /// </summary>
        public string? UserAgent { get; init; }

        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public required string DeviceId { get; init; }

        /// <summary>
        /// Region
        /// 区域
        /// </summary>
        public required string Region { get; init; }

        /// <summary>
        /// Timezone
        /// 时区
        /// </summary>
        public string? Timezone { get; init; }
    }
}
