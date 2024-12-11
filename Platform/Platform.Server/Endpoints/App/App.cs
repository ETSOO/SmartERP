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

            g.MapPost("GetMy", (IAppService service, AppGetMyRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.GetMyAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get my applications JSON data / 获取我的应用JSON数据").WithTags("App");

            g.MapPost("List", (IAppService service, AppListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("List applications JSON data / 列出应用JSON数据").WithTags("App");

            g.MapPost("Query", (IAppService service, AppQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query applications JSON data / 查询应用JSON数据").WithTags("App");

            g.MapPost("QueryPurchased", (IAppService service, AppPurchasedQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryPurchasedAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query purchased applications JSON data / 查询已购应用JSON数据").WithTags("App");

            return builder;
        }
    }
}
