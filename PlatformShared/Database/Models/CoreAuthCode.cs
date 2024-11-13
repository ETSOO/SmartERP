using System.Net;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Core authentication code
    /// 核心认证码
    /// </summary>
    public class CoreAuthCode
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Core user id
        /// 核心用户编号
        /// </summary>
        public int? CoreUserId { get; init; }

        /// <summary>
        /// Code action
        /// 认证码动作
        /// </summary>
        public short Action { get; init; }

        /// <summary>
        /// Openid
        /// 公开编号
        /// </summary>
        public required string Openid { get; init; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTime Expiry { get; init; }

        /// <summary>
        /// IP address
        /// IP地址
        /// </summary>
        public required IPAddress Ip { get; init; }

        /// <summary>
        /// Hashed authorization code
        /// 哈希授权码
        /// </summary>
        public required string Code { get; init; }

        /// <summary>
        /// Times
        /// 次数
        /// </summary>
        public short Times { get; set; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Core user
        /// 核心用户
        /// </summary>
        public CoreUser? CoreUser { get; init; }
    }
}
