using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using System.Text.Json;

namespace Admin.Server.RQ.Document
{
    /// <summary>
    /// Document create request data
    /// 文档创建请求数据
    /// </summary>
    public record DocumentCreateRQ : IModelValidator
    {
        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public required string Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Parameters
        /// 参数
        /// </summary>
        public JsonDocument? Parameters { get; init; }

        /// <summary>
        /// Template
        /// 模板
        /// </summary>
        public required string Template { get; init; }

        /// <summary>
        /// Cultures
        /// 语言文化
        /// </summary>
        public IEnumerable<string>? Cultures { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Kind.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Kind));
            }

            if (Title.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (Cultures != null)
            {
                var lc = new LanguageCodeAttribute();
                if (Cultures.Any(c => !lc.IsValid(c)))
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(Cultures));
                }
            }

            return null;
        }
    }
}
