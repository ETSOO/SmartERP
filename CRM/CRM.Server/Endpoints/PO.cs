using com.etsoo.WebUtils;
using CRM.Server.RQ.PO;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Purchase order service APIs
    /// 采购服务API
    /// </summary>
    internal static class PO
    {
        public static RouteGroupBuilder MapPO(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("PO");

            g.MapPost("List", (IPOService service, POListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get purchase order list / 获取采购列表").WithTags("PO");

            g.MapPost("Query", (IPOService service, POQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query purchase order info / 查询采购信息").WithTags("PO");

            return builder;
        }
    }
}
