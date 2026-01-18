using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using CRM.Server.Dto.System;

namespace CRM.Server.RQ.System
{
    /// <summary>
    /// Read culture request data
    /// 读取请求数据
    /// </summary>
    public record ReadCultureRQ : IModelValidator
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public required string Culture { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public CustomCultureKind Kind { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (!new LanguageCodeAttribute().IsValid(Culture))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Culture));
            }

            return null;
        }
    }
}
