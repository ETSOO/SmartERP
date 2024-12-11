using com.etsoo.CoreFramework.Business;

namespace Platform.Server.Endpoints.App.RQ
{
    /// <summary>
    /// Application list request data
    /// 应用列表请求数据
    /// </summary>
    public record AppListRQ : QueryIntRQ
    {
        /// <summary>
        /// Identity type
        /// 身份类型
        /// </summary>
        public IdentityType? IdentityType { get; init; }

        /// <summary>
        /// Require local URL or not
        /// 是否需要本地地址
        /// </summary>
        public bool? RequireLocalUrl { get; init; }
    }
}
