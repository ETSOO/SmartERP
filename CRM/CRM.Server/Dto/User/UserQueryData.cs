using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.User
{
    /// <summary>
    /// User query data
    /// 用户查询数据
    /// </summary>
    public record UserQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// User role
        /// 用户角色
        /// </summary>
        public UserRole? UserRole { get; init; }

        /// <summary>
        /// Departments
        /// 所属部门
        /// </summary>
        public IEnumerable<string>? Depts { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Editable
        /// 可编辑
        /// </summary>
        public bool Editable { get; init; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
