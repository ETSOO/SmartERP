using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.String;
using com.etsoo.WebUtils.Attributes;
using CRM.Server.Dto.System;

namespace CRM.Server.RQ.System
{
    /// <summary>
    /// Update custom culture
    /// 更新自定义文化
    /// </summary>
    public record UpdateCultureRQ : IModelValidator
    {
        /// <summary>
        /// Related Id
        /// 关联编号
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
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Json data
        /// JSON 数据
        /// </summary>
        public string? JsonData { get; init; }

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

            if (Title != null && Title.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 2560))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (JsonData != null && !JsonData.IsJson())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(JsonData));
            }

            return null;
        }
    }
}
