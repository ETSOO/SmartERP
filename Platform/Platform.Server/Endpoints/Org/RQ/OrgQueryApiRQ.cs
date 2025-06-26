using PlatformShared.Database.Models;

namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Organization query API request data
    /// 机构查询接口请求数据
    /// </summary>
    public record OrgQueryApiRQ : QueryIntRQ, IOrgRQ
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
        public CoreApiService? Service { get; init; }

        /// <summary>
        /// App or user id
        /// 程序或用户编号
        /// </summary>
        public string? AppId { get; init; }
    }
}
