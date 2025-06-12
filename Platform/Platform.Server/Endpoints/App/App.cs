using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.Web;
using com.etsoo.WebUtils;
using Platform.Server.Endpoints.App.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.App
{
    /// <summary>
    /// Application service APIs
    /// 程序服务API
    /// </summary>
    public static class App
    {
        public static RouteGroupBuilder MapApp(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("App");

            g.MapPost("Buy", [Roles(Constants.FinanceRoles)] (IAppService service, AppBuyRQ rq, CancellationToken cancellationToken) => service.BuyAsync(rq, cancellationToken))
                .WithDescription("Buy application / 购买应用").WithTags("App");

            g.MapPost("BuyNew", [Roles(Constants.FinanceRoles)] (IAppService service, AppBuyNewRQ rq, CancellationToken cancellationToken) => service.BuyNewAsync(rq, cancellationToken))
                .WithDescription("Buy application with creating organization / 购买应用并创建机构").WithTags("App");

            g.MapPut("CreateApiKey", [Roles(Constants.AdminRoles)] async (IAppService service, IHttpContextAccessor accessor, AppCreateApiKeyRQ rq, CancellationToken cancellationToken) =>
            {
                // Check device
                if (!service.CheckDevice(accessor.UserAgent(), rq.DeviceId, out var checkResult, out var cd))
                {
                    return checkResult;
                }

                var deviceCore = cd.Value.DeviceCore;

                return await service.CreateApiKeyAsync(rq.Id, deviceCore, cancellationToken);
            }).WithDescription("Create API key / 创建API密钥").WithTags("App");

            g.MapPost("GetMy", (IAppService service, AppGetMyRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.GetMyAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get my applications JSON data / 获取我的应用JSON数据").WithTags("App");

            g.MapPost("List", (IAppService service, AppListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("List applications JSON data / 列出应用JSON数据").WithTags("App");

            g.MapPost("Query", (IAppService service, AppQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query applications JSON data / 查询应用JSON数据").WithTags("App");

            g.MapPost("QueryPurchased", [Roles(Constants.FinanceRoles)] (IAppService service, AppPurchasedQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryPurchasedAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query purchased applications JSON data / 查询已购应用JSON数据").WithTags("App");

            g.MapGet("Read/{id:int}", (IAppService service, int id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read app JSON data / 浏览应用JSON数据").WithTags("App");

            g.MapPut("Renew", [Roles(Constants.FinanceRoles)] (IAppService service, AppRenewRQ rq, CancellationToken cancellationToken) => service.RenewAsync(rq, cancellationToken))
                .WithDescription("Renew application / 应用续费").WithTags("App");

            g.MapPut("Update", [Roles(Constants.AdminRoles)] (IAppService service, AppUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update application / 更新应用").WithTags("App");

            g.MapGet("UpdateRead/{id:int}", [Roles(Constants.AdminRoles)] (IAppService service, int id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.UpdateReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read JSON data for upate / 浏览JSON数据用于更新").WithTags("App");

            return builder;
        }
    }
}
