using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.PersonProfile
{
    /// <summary>
    /// Person profile attachment update request data
    /// 人员档案附件更新请求数据
    /// </summary>
    public record PersonProfileAttachmentUpdateRQ : IModelValidator
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public required long Id { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public required string Description { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Description.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            return null;
        }
    }
}
