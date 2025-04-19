using com.etsoo.ApiModel.RQ.SmartERP;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.PersonProfile
{
    /// <summary>
    /// Person profile link update request data
    /// 人员档案关联更新请求数据
    /// </summary>
    public record PersonProfileLinkUpdateRQ : UpdateModel<long>, IModelValidator
    {
        /// <summary>
        /// Token auth data
        /// 令牌认证数据
        /// </summary>
        public required TokenAuthRQ Auth { get; init; }

        /// <summary>
        /// Person profile id
        /// 人员档案编号
        /// </summary>
        public long? ProfileId { get; init; }

        /// <summary>
        /// Target profile id
        /// 关联的人员档案编号
        /// </summary>
        public long? TargetProfileId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonProfileLinkKind? Kind { get; init; }

        /// <summary>
        /// Content
        /// 内容
        /// </summary>
        public string? Content { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (TargetProfileId.HasValue && ProfileId.HasValue && TargetProfileId.Value == ProfileId.Value)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(TargetProfileId));
            }

            return null;
        }
    }
}
