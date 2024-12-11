using com.etsoo.WebUtils;
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

            g.MapPost("Query", (IMemberService service, MemberQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query members JSON data / 查询成员JSON数据").WithTags("Member");

            return builder;
        }
    }
}
