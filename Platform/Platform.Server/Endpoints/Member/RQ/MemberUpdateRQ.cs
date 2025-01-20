using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.Member.RQ
{
    /// <summary>
    /// Update member request data
    /// 更新成员请求数据
    /// </summary>
    public record MemberUpdateRQ : UpdateModel<int>, IModelValidator
    {
        /// <summary>
        /// User role
        /// 用户角色
        /// </summary>
        public UserRole? UserRole { get; init; }

        /// <summary>
        /// Local name
        /// 本地名称
        /// </summary>
        public string? LocalName { get; set; }

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
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (LocalName != null && LocalName.Length is not (>= 2 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(LocalName));
            }

            if (AssignedId != null && AssignedId.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            return null;
        }
    }
}
