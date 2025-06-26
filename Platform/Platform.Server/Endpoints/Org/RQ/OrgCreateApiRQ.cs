using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.String;
using PlatformShared.Database.Models;

namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Create API request data
    /// 创建接口请求数据
    /// </summary>
    public record OrgCreateApiRQ : IModelValidator, IOrgRQ
    {
        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; set; }

        /// <summary>
        /// Service
        /// 服务
        /// </summary>
        public CoreApiService Service { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Endpoint URL
        /// 端点网址
        /// </summary>
        public string? Endpoint { get; init; }

        /// <summary>
        /// App or user ID
        /// 程序或用户编号
        /// </summary>
        public required string AppId { get; init; }

        /// <summary>
        /// App or user secret
        /// 程序或用户密钥
        /// </summary>
        public required string AppSecret { get; init; }

        /// <summary>
        /// JSON options
        /// JSON 选项
        /// </summary>
        public string? Options { get; init; }

        /// <summary>
        /// Rate policy
        /// 频次政策
        /// </summary>
        public short? RatePolicy { get; init; }

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool? Enabled { get; init; }

        /// <summary>
        /// Inheritance or not
        /// 是否继承
        /// </summary>
        public bool? Inheritance { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Title.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (Endpoint != null && Endpoint.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Endpoint));
            }

            if (AppId.Length is not (>= 1 and <= 64))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AppId));
            }

            if (AppSecret.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AppSecret));
            }

            if (Options != null && !Options.IsJson())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Options));
            }

            return null;
        }
    }
}
