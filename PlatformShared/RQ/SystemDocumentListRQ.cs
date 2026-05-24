using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;

namespace PlatformShared.RQ
{
    /// <summary>
    /// System Document list request data
    /// 系统文档列表请求数据
    /// </summary>
    public record SystemDocumentListRQ : QueryRQ<int>, IModelValidator
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public string? Kind { get; init; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public string? Culture { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public override IActionResult? Validate()
        {
            var result = base.Validate();
            if (result?.Ok is false)
            {
                return result;
            }

            if (Kind != null && Kind.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Kind));
            }

            if (Culture != null && new LanguageCodeAttribute().IsValid(Culture) is false)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Culture));
            }

            return null;
        }
    }
}
