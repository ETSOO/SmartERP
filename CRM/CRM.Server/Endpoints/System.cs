using CRM.Server.RQ.System;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    internal static class System
    {
        /// <summary>
        /// Map system APIs
        /// 映射系统API
        /// </summary>
        /// <param name="builder">Route group builder</param>
        /// <returns>Route group builder</returns>
        public static RouteGroupBuilder MapSystem(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("System");

            g.MapGet("PermissionItems", (ISystemService service, CancellationToken cancellationToken) => service.PermissionItemsAsync(cancellationToken))
                .WithDescription("Get all permission items / 获取所有权限项").WithTags("System");

            g.MapPost("ReadCulture", (ISystemService service, ReadCultureRQ rq, CancellationToken cancellationToken) => service.ReadCultureAsync(rq, cancellationToken))
                .WithDescription("Read custom culture / 读取自定义文化").WithTags("System");

            g.MapGet("ReadSettings", (ISystemService service, CancellationToken cancellationToken) => service.ReadSettingsAsync(cancellationToken))
                .WithDescription("Read system settings / 读取系统设置").WithTags("System");

            g.MapPut("UpdateCulture", (ISystemService service, UpdateCultureRQ rq, CancellationToken cancellationToken) => service.UpdateCultureAsync(rq, cancellationToken))
                .WithDescription("Update custom culture / 更新自定义文化").WithTags("System");

            g.MapPut("UpdateSettings", (ISystemService service, UpdateSettingsRQ rq, CancellationToken cancellationToken) => service.UpdateSettingsAsync(rq, cancellationToken))
                .WithDescription("Update system settings / 更新系统设置").WithTags("System");

            return builder;
        }
    }
}
