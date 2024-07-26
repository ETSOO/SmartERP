using System.ComponentModel.DataAnnotations;

namespace Platform.Server.Database.Models
{
    /// <summary>
    /// Core user identifier type
    /// 核心用户编号类型
    /// </summary>
    public enum CoreUserIdentifierType : byte
    {
        Email = 1,
        Mobile = 2,
        Wechat = 3,
        Alipay = 4,
        Google = 5,
        Microsoft = 6
    }

    /// <summary>
    /// Core user identifier verifed for login
    /// 核心用户已验证的登录编号
    /// </summary>
    public class CoreUserIdentifier
    {
        /// <summary>
        /// Identifier
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Core user id
        /// 核心用户编号
        /// </summary>
        public int CoreUserId { get; init; }

        /// <summary>
        /// Identifier type
        /// 编号类型
        /// </summary>
        public required CoreUserIdentifierType Type { get; init; }

        /// <summary>
        /// Identifier value
        /// 编号值
        /// </summary>
        [Required]
        [StringLength(256)]
        public required string Value { get; init; }

        /// <summary>
        /// Reference type for the identifier, like Google email for Google account
        /// 编号的引用类型，比如谷歌邮箱对应的谷歌账号
        /// </summary>
        public CoreUserIdentifierType? RefType { get; init; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Core user
        /// 核心用户
        /// </summary>
        public virtual CoreUser CoreUser { get; init; } = default!;
    }
}
