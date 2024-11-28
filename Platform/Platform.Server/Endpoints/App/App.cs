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

            g.MapGet("GetUserLatestApp", (IAppService service, CancellationToken cancellationToken) => service.GetUserLatestAppAsync(cancellationToken))
                .WithDescription("Get user's latest accessed appliation's Web URL / 获取用户最近访问的程序的Web网址").WithTags("App");

            g.MapGet("GetUserApps", (IAppService service, CancellationToken cancellationToken) => service.GetUserAppsAsync(cancellationToken))
                .WithDescription("Get user applications / 获取用户所有程序").WithTags("App");

            return builder;
        }
    }
}
