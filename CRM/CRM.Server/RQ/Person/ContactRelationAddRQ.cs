using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.String;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Person
{
    /// <summary>
    /// Contact relation add request data
    /// 添加联系人关系请求数据
    /// </summary>
    public record ContactRelationAddRQ : IModelValidator
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init;  }

        /// <summary>
        /// Contact's person id
        /// 联系人的人员编号
        /// </summary>
        public long ContactId { get; init; }

        /// <summary>
        /// Relation type
        /// 关系类型
        /// </summary>
        public PersonRelationType RelationType { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

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
            if (Description != null && Description.Length > 128)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (Data != null && !Data.IsJson())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Data));
            }

            return null;
        }
    }
}
