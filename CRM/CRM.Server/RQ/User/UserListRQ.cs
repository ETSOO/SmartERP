using System.Text.Json.Serialization;

namespace CRM.Server.RQ.User
{
    /// <summary>
    /// User list request data
    /// 用户列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(UserQueryRQ))]
    public record UserListRQ : QueryLongRQ
    {
        /// <summary>
        /// Department ID
        /// 部门编号
        /// </summary>
        public long? DeptId { get; init; }

        /// <summary>
        /// Permission group ID
        /// 权限组编号
        /// </summary>
        public int? GroupId { get; init; }
    }
}
