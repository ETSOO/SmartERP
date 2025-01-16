using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.App.RQ
{
    /// <summary>
    /// App update request data
    /// 应用更新请求数据
    /// </summary>
    public record AppUpdateRQ : UpdateModel<int>, IModelValidator
    {
        /// <summary>
        /// Local name
        /// 本地名称
        /// </summary>
        public string? LocalName { get; init; }

        /// <summary>
        /// Local Web URL
        /// 本地网址
        /// </summary>
        public string? LocalUrl { get; init; }

        /// <summary>
        /// Local help URL
        /// 本地帮助网址
        /// </summary>
        public string? LocalHelpUrl { get; init; }

        /// <summary>
        /// Local APIs
        /// 本地接口
        /// </summary>
        public IEnumerable<string>? LocalApis { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (LocalName != null && LocalName.Length is < 2 or > 128)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(LocalName));
            }

            if (LocalUrl != null && !Uri.IsWellFormedUriString(LocalUrl, UriKind.Absolute))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(LocalUrl));
            }

            if (LocalHelpUrl != null && !Uri.IsWellFormedUriString(LocalHelpUrl, UriKind.Absolute))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(LocalHelpUrl));
            }

            if (LocalApis != null && LocalApis.Any(a => !Uri.IsWellFormedUriString(a, UriKind.Absolute)))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(LocalApis));
            }

            return null;
        }
    }
}
