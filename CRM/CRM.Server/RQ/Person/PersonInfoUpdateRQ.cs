using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Person
{
    /// <summary>
    /// Person info update request data
    /// 人员信息更新请求数据
    /// </summary>
    public record PersonInfoUpdateRQ : UpdateModel<int>, IModelValidator
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonInfoKind Kind { get; init; }

        /// <summary>
        /// Identifier
        /// 标识
        /// </summary>
        public string? Identifier { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Is default or not
        /// 是否默认
        /// </summary>
        public bool? IsDefault { get; init; }

        /// <summary>
        /// Is subscribed or not
        /// 是否订阅
        /// </summary>
        public bool? Subscribed { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Identifier != null)
            {
                return RQExtentions.ValidatePersonInfo(Kind, Identifier);
            }

            if (Description != null && Description.Length > 256)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            return null;
        }
    }
}