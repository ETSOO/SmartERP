using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.String;
using System.Text.Json;

namespace CRM.Server.RQ.Customer
{
    /// <summary>
    /// Update customer request data
    /// 更新客户请求数据
    /// </summary>
    public record CustomerUpdateRQ : UpdateModel<long>, IModelValidator
    {
        /// <summary>
        /// Is legal person (enterprise)
        /// 是否为法人（企业）
        /// </summary>
        public bool? IsLegalPerson { get; init; }

        /// <summary>
        /// Name
        /// 名称 / 姓名
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Preferred name
        /// 首先名
        /// </summary>
        public string? PreferredName { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// PIN
        /// 身份编号
        /// </summary>
        public string? Pin { get; init; }

        /// <summary>
        /// Tax ID
        /// 税号
        /// </summary>
        public string? TaxId { get; init; }

        /// <summary>
        /// Birthday
        /// 生日
        /// </summary>
        public DateTimeOffset? Birthday { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<int>? Categories { get; init; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        public IEnumerable<string>? Tags { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public JsonDocument? Data { get; init; }

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

            if (PreferredName != null && PreferredName.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(PreferredName));
            }

            if (AssignedId != null && AssignedId.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            if (Pin != null && Pin.Length is not (>= 6 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Pin));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 1280))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (Tags != null && Tags.Any(t => t.Length is < 1 or > 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Tags));
            }

            return null;
        }
    }
}
