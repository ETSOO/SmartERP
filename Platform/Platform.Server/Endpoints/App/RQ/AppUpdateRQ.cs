using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Dto;

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
        /// Local URLs
        /// 本地网址
        /// </summary>
        public AppUrl[]? LocalUrls { get; init; }

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

            if (LocalUrls != null && LocalUrls.Any(u => !Uri.IsWellFormedUriString(u.Web, UriKind.Absolute)
                || !Uri.IsWellFormedUriString(u.Api, UriKind.Absolute)
                || (u.Help != null && !Uri.IsWellFormedUriString(u.Help, UriKind.Absolute))
            ))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(LocalUrls));
            }

            return null;
        }
    }
}
