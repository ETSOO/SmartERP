using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;

namespace CRM.Server.Dto.User
{
    /// <summary>
    /// User update read data
    /// 用户更新读取数据
    /// </summary>
    public record UserUpdateReadData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// User role, permission level
        /// 用户角色，权限等级
        /// </summary>
        public UserRole? UserRole { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Report to (person.id)
        /// 汇报对象
        /// </summary>
        public long? ReportTo { get; init; }

        /// <summary>
        /// Expiry time
        /// 到期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Department ids
        /// 所属部门编号
        /// </summary>
        public IEnumerable<LongIdItem>? Depts { get; init; }

        /// <summary>
        /// Permission group ids
        /// 所属权限组编号
        /// </summary>
        public IEnumerable<int>? Groups { get; init; }

        /// <summary>
        /// Permission items included
        /// 包含的权限项目
        /// </summary>
        public IEnumerable<short>? PermissionIncluded { get; init; }

        /// <summary>
        /// Permission items excluded
        /// 排除的权限项目
        /// </summary>
        public IEnumerable<short>? PermissionExcluded { get; init; }
    }
}
