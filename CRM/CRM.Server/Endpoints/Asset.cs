using com.etsoo.WebUtils;
using CRM.Server.RQ.Asset;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Asset service APIs
    /// 资产服务API
    /// </summary>
    internal static class Asset
    {
        public static RouteGroupBuilder MapAsset(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Asset");

            g.MapPost("List", (IAssetService service, AssetListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get asset list / 获取资产列表").WithTags("Asset");

            g.MapPost("Query", (IAssetService service, AssetQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query asset info / 查询资产信息").WithTags("Asset");

            return builder;
        }
    }
}
