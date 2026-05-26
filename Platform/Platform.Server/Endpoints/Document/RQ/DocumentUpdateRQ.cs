using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using Platform.Server.Endpoints.Org.RQ;
using System.Text.Json;

namespace Platform.Server.Endpoints.Document.RQ
{
    /// <summary>
    /// Document update request data
    /// 文档更新请求数据
    /// </summary>
    public record DocumentUpdateRQ : UpdateModel<int>, IModelValidator, IOrgRQ
    {
        /// <summary>
        /// Organizaton id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public string? Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Parameters
        /// 参数
        /// </summary>
        public JsonDocument? Parameters { get; init; }

        /// <summary>
        /// Template
        /// 模板
        /// </summary>
        public string? Template { get; init; }

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
            if (Kind != null && Kind.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Kind));
            }

            if (Title != null && Title.Length is not (>= 1 and <= 128))
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
