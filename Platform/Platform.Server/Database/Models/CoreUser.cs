using com.etsoo.CoreFramework.Business;
using System.ComponentModel.DataAnnotations;

namespace Platform.Server.Database.Models
{
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
        [Required]
        public required int Id { get; init; }

        /// <summary>
        /// Password
        /// 密码
        /// </summary>
        [StringLength(128)]
        public string? Password { get; init; }

        /// <summary>
        /// Display name
        /// 显示名称
        /// </summary>
        [Required]
        [StringLength(128)]
        public required string Name { get; init; }

        /// <summary>
        /// Given name
        /// 名
        /// </summary>
        [StringLength(50)]
        public string? GivenName { get; init; }

        /// <summary>
        /// Family name
        /// 姓
        /// </summary>
        [StringLength(50)]
        public string? FamilyName { get; init; }

        /// <summary>
        /// Foreign name
        /// 外文名称
        /// </summary>
        [StringLength(128)]
        public string? ForeignName { get; init; }

        /// <summary>
        /// Avatar
        /// 头像
        /// </summary>
        [StringLength(256)]
        public string? Avatar { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        [StringLength(20)]
        public string? AssignedId { get; init; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; init; } = DateTimeOffset.Now;

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; } = EntityStatus.Normal;

        /// <summary>
        /// Frozen expiry time
        /// 冻结到期时间
        /// </summary>
        public DateTime? FrozenTime { get; init; }

        /// <summary>
        /// Identities
        /// 唯一标识
        /// </summary>
        public IEnumerable<CoreUserIdentifier>? CoreUserIdentifiers { get; init; }
    }
}
