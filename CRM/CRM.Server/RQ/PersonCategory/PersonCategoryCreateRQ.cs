using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.String;

namespace CRM.Server.RQ.PersonCategory
{
    /// <summary>
    /// Person category create request data
    /// 人员分类创建请求数据
    /// </summary>
    public record PersonCategoryCreateRQ : IModelValidator
    {
        /// <summary>
        /// Identity type
        /// 识别类型
        /// </summary>
        public required IdentityTypeFlags IdentityType { get; init; }

        /// <summary>
        /// Parent Id
        /// 父级编号
        /// </summary>
        public int? ParentId { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

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
            if (Name.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (AssignedId !=null && AssignedId.Length is not (>= 1 and <= 20))
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
