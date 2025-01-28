using com.etsoo.CoreFramework.Models;
using com.etsoo.WebUtils;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Platform.Server.Endpoints.Org.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.Org
{
    /// <summary>
    /// Organization service APIs
    /// 机构服务API
    /// </summary>
    public static class Org
    {
        public static RouteGroupBuilder MapOrg(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Org");

            g.MapPut("Create", (IOrgService service, OrgCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create organization / 创建新机构").WithTags("Org");

            g.MapDelete("Delete/{id:int}", (IOrgService service, int id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete organization / 删除机构").WithTags("Org");

            g.MapPost("GetMy", (IOrgService service, OrgGetMyRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.GetMyAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get my organizations JSON data / 获取我的机构JSON数据").WithTags("Org");

            g.MapPost("Leave/{id:int}", (IOrgService service, int id, CancellationToken cancellationToken) => service.LeaveAsync(id, cancellationToken))
                .WithDescription("Leave organization / 离开机构").WithTags("Org");

            g.MapPost("List", (IOrgService service, OrgListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("List organizations JSON data / 列出机构JSON数据").WithTags("Org");

            g.MapPost("Query", (IOrgService service, OrgQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query organizations JSON data / 查询机构JSON数据").WithTags("Org");

            g.MapGet("Read/{id:int}", (IOrgService service, int id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read organizations JSON data / 浏览机构JSON数据").WithTags("Org");

            g.MapGet("RequestToken", (IAntiforgery forgeryService, IHttpContextAccessor accessor) =>
            {
                // Create the token
                var token = forgeryService.GetAndStoreTokens(accessor.HttpContext!);

                // Return the token
                return new AntiforgeryRequestToken { Name = token.FormFieldName, HeaderName = token.HeaderName, Value = token.RequestToken };
            }).WithDescription("Get Antiforgery request token / 获取反伪造请求令牌").WithTags("Org");

            g.MapPut("Update", (IOrgService service, OrgUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update organization / 更新机构").WithTags("Org");

            g.MapPut("UpdateAvatar/{id:int}", (IOrgService service, [FromRoute] int id, [FromForm] IFormFile avatar, CancellationToken cancellationToken) => service.UpdateAvatarAsync(id, avatar.OpenReadStream(), avatar.ContentType, cancellationToken))
                .DisableAntiforgery()
                .WithDescription("Update organization avatar / 更新机构头像").WithTags("Org");

            g.MapGet("UpdateRead/{id:int}", (IOrgService service, int id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.UpdateReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read JSON data for upate / 浏览JSON数据用于更新").WithTags("Org");

            return builder;
        }
    }
}
