using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.RQ;
using PlatformShared.Database.Models;

namespace CRM.Server.Dto.PersonInfo
{
    /// <summary>
    /// Person info item
    /// 个人信息项
    /// </summary>
    public partial record PersonInfoItem : IModelValidator
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
        public required string Identifier { get; init; }

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
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Description != null && Description.Length > 256)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            return RQExtentions.ValidatePersonInfo(Kind, Identifier);
        }
    }
}
