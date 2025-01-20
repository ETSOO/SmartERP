using com.etsoo.WebUtils;
using Microsoft.AspNetCore.Mvc;
using Platform.Server.Endpoints.Member.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.Member
{
    /// <summary>
    /// Member service APIs
    /// 成员服务API
    /// </summary>
    public static class Member
    {
        public static RouteGroupBuilder MapMember(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Member");

            g.MapDelete("Delete/{id:int}", (IMemberService service, int id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete member / 删除成员").WithTags("Member");

            g.MapPost("List", (IMemberService service, MemberListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("List members JSON data / 列出成员JSON数据").WithTags("Member");

            g.MapPost("Invite", (IMemberService service, MemberInviteRQ rq, CancellationToken cancellationToken) => service.InviteAsync(rq))
                .WithDescription("Invite member / 邀请成员").WithTags("Member");

            g.MapPost("Query", (IMemberService service, MemberQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query members JSON data / 查询成员JSON数据").WithTags("Member");

            g.MapGet("Read/{id:int}", (IMemberService service, int id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read member JSON data / 浏览成员JSON数据").WithTags("Member");

            g.MapPut("Update", (IMemberService service, MemberUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update member / 更新成员").WithTags("Member");

            g.MapPut("UpdateAvatar/{id:int}", (IMemberService service, [FromRoute] int id, [FromForm] IFormFile avatar, CancellationToken cancellationToken) => service.UpdateAvatarAsync(id, avatar.OpenReadStream(), avatar.ContentType, cancellationToken))
                .DisableAntiforgery()
                .WithDescription("Update member avatar / 更新成员头像").WithTags("Member");

            g.MapGet("UpdateRead/{id:int}", (IMemberService service, int id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.UpdateReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read JSON data for upate / 浏览JSON数据用于更新").WithTags("Member");

            return builder;
        }
    }
}
