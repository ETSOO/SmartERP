using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.String;
using System.Text.Json;

namespace CRM.Server.RQ.User
{
    /// <summary>
    /// User update request data
    /// 用户更新请求数据
    /// </summary>
    public record UserUpdateRQ : UpdateModel<long>, IModelValidator
    {
        /// <summary>
        /// User role
        /// 用户角色
        /// </summary>
        public UserRole? UserRole { get; init; }

        /// <summary>
        /// Name
        /// 姓名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; set; }

        /// <summary>
        /// Expiry
        /// 过期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; set; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// Report to
        /// 汇报对象
        /// </summary>
        public long? ReportTo { get; init; }

        /// <summary>
        /// Department ids
        /// 所属部门编号
        /// </summary>
        public IEnumerable<long>? Depts { get; init; }

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

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<int>? Categories { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public JsonDocument? Data { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Name != null && Name.Length is not (>= 2 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (AssignedId != null && AssignedId.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            return null;
        }
    }
}
