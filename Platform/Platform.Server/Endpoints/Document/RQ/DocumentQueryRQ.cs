using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using Platform.Server.Endpoints.Org.RQ;

namespace Platform.Server.Endpoints.Document.RQ
{
    /// <summary>
    /// Document query request data
    /// 文档查询请求数据
    /// </summary>
    public record DocumentQueryRQ : QueryIntRQ, IModelValidator, IOrgRQ
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
        /// Culture
        /// 文化
        /// </summary>
        public string? Culture { get; init; }

        /// <summary>
        /// System template or not
        /// 是否为系统模板
        /// </summary>
        public bool? IsSystem { get; init; }

        /// <summary>
        /// Has parameters or not
        /// 是否有参数
        /// </summary>
        public bool? HasParameters { get; init; }

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
