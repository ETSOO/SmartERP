using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.Order
{
    /// <summary>
    /// Order duplicate test request data
    /// 订单重复测试请求数据
    /// </summary>
    public record OrderDuplicateTestRQ : IModelValidator
    {
        /// <summary>
        /// Excluded id
        /// 排除的编号
        /// </summary>
        public long? ExcludedId { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Source id
        /// 源编号
        /// </summary>
        public string? SourceId { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Title != null && Title.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (AssignedId != null && AssignedId.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            if (SourceId != null && SourceId.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(SourceId));
            }

            return null;
        }
    }
}
