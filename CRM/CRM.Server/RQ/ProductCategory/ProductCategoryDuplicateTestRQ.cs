using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;

namespace CRM.Server.RQ.ProductCategory
{
    /// <summary>
    /// Product category duplicate test request data
    /// 产品分类重复测试请求数据
    /// </summary>
    public record ProductCategoryDuplicateTestRQ
    {
        /// <summary>
        /// Excluded id
        /// 排除的编号
        /// </summary>
        public int? ExcludedId { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

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
