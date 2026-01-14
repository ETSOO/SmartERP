using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.String;

namespace CRM.Server.RQ.ProductCategory
{
    /// <summary>
    /// Product category update request data
    /// 产品分类更新请求数据
    /// </summary>
    public record ProductCategoryUpdateRQ : UpdateModel<int>, IModelValidator
    {
        /// <summary>
        /// Parent Id
        /// 父级编号
        /// </summary>
        public int? ParentId { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Assigned ID
        /// 分配编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Order index
        /// 排序索引
        /// </summary>
        public short? OrderIndex { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Name != null && Name.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (AssignedId != null && AssignedId.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            if (Data != null && !Data.IsJson())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Data));
            }

            return null;
        }
    }
}
