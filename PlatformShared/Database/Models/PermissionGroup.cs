using com.etsoo.CoreFramework.Authentication;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Permission group
    /// 权限组
    /// </summary>
    public class PermissionGroup
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// User roles
        /// 用户角色
        /// </summary>
        public UserRole Roles { get; set; }

        /// <summary>
        /// Items
        /// 所有项目
        /// </summary>
        public List<short> Items { get; set; } = default!;

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int? CoreOrganizationId { get; set; }

        /// <summary>
        /// Core organization
        /// 核心机构
        /// </summary>
        public CoreOrganization? CoreOrganization { get; set; }
    }
}
