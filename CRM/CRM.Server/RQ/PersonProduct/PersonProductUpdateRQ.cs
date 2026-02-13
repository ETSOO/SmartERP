using com.etsoo.CoreFramework.Application;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using PlatformShared.Dto;

namespace CRM.Server.RQ.PersonProduct
{
    /// <summary>
    /// Person product update request data
    /// 人员个性化产品更新请求数据
    /// </summary>
    public record PersonProductUpdateRQ : IUpdateModel, IModelValidator
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public long ProductId { get; init; }

        /// <summary>
        /// Assigned ID
        /// 分配编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Json data
        /// JSON 数据
        /// </summary>
        public PersonProductJsonData? JsonData { get; init; }

        /// <summary>
        /// Changed fields
        /// 变更字段
        /// </summary>
        public IEnumerable<string>? ChangedFields { get; set; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (AssignedId != null && AssignedId.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            if (JsonData != null)
            {
                if (JsonData.Cultures != null && JsonData.Cultures.Any(c => !c.Validate()))
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(JsonData.Cultures));
                }

                if (JsonData.Prices != null && JsonData.Prices.Any(c => !c.Validate()))
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(JsonData.Prices));
                }
            }

            return null;
        }
    }
}
