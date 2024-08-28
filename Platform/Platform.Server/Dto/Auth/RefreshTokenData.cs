namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Refresh token data
    /// 刷新令牌数据
    /// </summary>
    public record RefreshTokenData
    {
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
        /// Refresh token
        /// 刷新令牌
        /// </summary>
        public required string Token { get; init; }

        /// <summary>
        /// Country or region code, like CN = China
        /// 国家或地区编号，如 CN = 中国
        /// </summary>
        public required string Region { get; init; }

        /// <summary>
        /// Relogin password
        /// 重新登录密码
        /// </summary>
        public string? Password { get; init; }
    }
}
