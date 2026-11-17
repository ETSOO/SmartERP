using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Admin.Server.RQ.Admin
{
    /// <summary>
    /// Merge accounts request data
    /// 合并账户请求数据
    /// </summary>
    public record MergeAccountsRQ : IModelValidator
    {
        /// <summary>
        /// Retained user id
        /// 保留的用户编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Merged user id
        /// 合并的用户编号
        /// </summary>
        public int MergedUserId { get; init; }

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
            if (MergedUserId == Id)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(MergedUserId));
            }

            if (Description.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            return null;
        }
    }
}
