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

            g.MapPost("Create", (IAssetService service, AssetCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create asset / 创建资产").WithTags("Asset");

            g.MapPost("List", (IAssetService service, AssetListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get asset list / 获取资产列表").WithTags("Asset");

            g.MapPost("Query", (IAssetService service, AssetQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query asset info / 查询资产信息").WithTags("Asset");

            g.MapGet("Read/{id:int}", (IAssetService service, int id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read asset info / 读取资产信息").WithTags("Asset");

            g.MapPost("ReadSensitiveData/{id:int}", (IAssetService service, int id, CancellationToken cancellationToken) => service.ReadSensitiveDataAsync(id, cancellationToken))
                .WithDescription("Read asset sensitive data / 读取资产敏感数据").WithTags("Asset");

            g.MapPut("Update", (IAssetService service, AssetUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update asset / 更新资产").WithTags("Asset");

            g.MapGet("UpdateRead/{id:int}", (IAssetService service, int id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Get asset update read data / 获取资产更新读取数据").WithTags("Asset");

            return builder;
        }
    }
}
