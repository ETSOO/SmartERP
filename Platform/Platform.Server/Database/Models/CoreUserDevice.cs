namespace Platform.Server.Database.Models
{
    /// <summary>
    /// Device type
    /// 设备类型
    /// </summary>
    public enum DeviceType : byte
    {
        /// <summary>
        /// Web
        /// 网页
        /// </summary>
        Web
    }

    /// <summary>
    /// Core user device
    /// 核心用户设备
    /// </summary>
    public class CoreUserDevice
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Belonged core user id
        /// 所属核心用户编号
        /// </summary>
        public int CoreUserId { get; set; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Device type
        /// 设备类型
        /// </summary>
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// Last login time
        /// 上次登录时间
        /// </summary>
        public DateTimeOffset LastLogin { get; set; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public required string Culture { get; set; }

        /// <summary>
        /// Refresh token
        /// 刷新令牌
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Refresh token expiry
        /// 刷新令牌过期时间
        /// </summary>
        public DateTimeOffset? RefreshTokenExpiry { get; set; }

        /// <summary>
        /// Client id
        /// 客户端编号
        /// </summary>
        public required string ClientId { get; set; }

        /// <summary>
        /// Core user
        /// 核心用户
        /// </summary>
        public CoreUser CoreUser { get; set; } = default!;
    }
}
