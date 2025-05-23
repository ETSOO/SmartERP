using com.etsoo.CoreFramework.Authentication;

namespace CRM.Server.Dto.Group
{
    /// <summary>
    /// Permission group view data
    /// 权限组浏览数据
    /// </summary>
    public record GroupViewData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// User roles
        /// 用户角色
        /// </summary>
        public UserRole Roles { get; init; }

        /// <summary>
        /// Permission items
        /// 权限项目
        /// </summary>
        public required IEnumerable<short> Items { get; init; }

        /// <summary>
        /// Organization id
        /// 所属机构编号
        /// </summary>
        public int? OrgId { get; init; }
    }
}
