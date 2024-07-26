using com.etsoo.CoreFramework.Business;
using System.ComponentModel.DataAnnotations;

namespace Platform.Server.Database.Models
{
    /// <summary>
    /// Core user registration step
    /// 核心用户注册步骤
    /// </summary>
    public static class CoreUserStep
    {
        /// <summary>
        /// Completed
        /// 已完成
        /// </summary>
        public const short Completed = 0;

        /// <summary>
        /// OAuth2
        /// 第三方登录
        /// </summary>
        public const short OAuth = 10;

        /// <summary>
        /// Email
        /// 邮箱
        /// </summary>
        public const short Email = 20;

        /// <summary>
        /// Mobile
        /// 手机号
        /// </summary>
        public const short Mobile = 30;

        /// <summary>
        /// Password
        /// 设置密码
        /// </summary>
        public const short Password = 40;

        /// <summary>
        /// Name
        /// 设置姓名
        /// </summary>
        public const short Name = 60;
    }

    /// <summary>
    /// Core user
    /// 核心用户
    /// </summary>
    public class CoreUser
    {
        /// <summary>
        /// Identifier
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Password
        /// 密码
        /// </summary>
        [StringLength(128)]
        public string? Password { get; set; }

        /// <summary>
        /// Display name
        /// 显示名称
        /// </summary>
        [Required]
        [StringLength(128)]
        public required string Name { get; set; }

        /// <summary>
        /// Given name
        /// 名
        /// </summary>
        [StringLength(50)]
        public string? GivenName { get; set; }

        /// <summary>
        /// Family name
        /// 姓
        /// </summary>
        [StringLength(50)]
        public string? FamilyName { get; set; }

        /// <summary>
        /// Foreign name
        /// 外文名称
        /// </summary>
        [StringLength(128)]
        public string? ForeignName { get; set; }

        /// <summary>
        /// Avatar
        /// 头像
        /// </summary>
        [StringLength(256)]
        public string? Avatar { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        [StringLength(20)]
        public string? AssignedId { get; set; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; } = EntityStatus.Normal;

        /// <summary>
        /// Frozen expiry time
        /// 冻结到期时间
        /// </summary>
        public DateTime? FrozenTime { get; set; }

        /// <summary>
        /// Registration step, 0 for completed
        /// 注册步骤，0为完成
        /// </summary>
        public short Step { get; set; }

        /// <summary>
        /// Core user identifiers
        /// 核心用户登录编号
        /// </summary>
        public virtual ICollection<CoreUserIdentifier> CoreUserIdentifiers { get; set; } = [];
    }
}