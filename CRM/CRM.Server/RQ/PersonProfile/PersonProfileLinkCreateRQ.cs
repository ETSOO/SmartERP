using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.PersonProfile
{
    /// <summary>
    /// Person profile link create request data
    /// 人员档案关联创建请求数据
    /// </summary>
    public record PersonProfileLinkCreateRQ : IModelValidator
    {
        /// <summary>
        /// Access token
        /// 访问令牌
        /// </summary>
        public required string AccessToken { get; init; }

        /// <summary>
        /// Token schema
        /// 令牌模式
        /// </summary>
        public string? TokenSchema { get; init; }

        /// <summary>
        /// Person profile id
        /// 人员档案编号
        /// </summary>
        public long ProfileId { get; init; }

        /// <summary>
        /// Target profile id
        /// 关联的人员档案编号
        /// </summary>
        public long? TargetProfileId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonProfileLinkKind Kind { get; init; } = PersonProfileLinkKind.Comment;

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
            if (TargetProfileId.HasValue && TargetProfileId.Value == ProfileId)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(TargetProfileId));
            }

            return null;
        }
    }
}
