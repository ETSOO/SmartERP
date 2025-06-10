using com.etsoo.WebUtils;
using CRM.Server.RQ.Tag;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Tag service APIs
    /// 标签服务API
    /// </summary>
    internal static class Tag
    {
        public static RouteGroupBuilder MapTag(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Tag");

            g.MapPost("List", (ITagService service, TagListRQ rq, CancellationToken cancellationToken) => service.ListAsync(rq, cancellationToken))
                .WithDescription("Get tag list / 获取标签列表").WithTags("Tag");

            g.MapPost("Query", (ITagService service, TagQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query tag info / 查询标签信息").WithTags("Tag");

            return builder;
        }
    }
}
