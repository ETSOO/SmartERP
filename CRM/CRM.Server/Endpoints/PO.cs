using com.etsoo.WebUtils;
using CRM.Server.RQ.PO;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// PO service APIs
    /// 订单服务API
    /// </summary>
    internal static class PO
    {
        public static RouteGroupBuilder MapPO(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("PO");

            g.MapPost("Create", (IPOService service, POCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create PO / 创建订单").WithTags("PO");

            g.MapPost("List", (IPOService service, POListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get PO list / 获取订单列表").WithTags("PO");

            g.MapPost("Query", (IPOService service, POQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query PO info / 查询订单信息").WithTags("PO");

            g.MapGet("Read/{id:long}", (IPOService service, long id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Get PO info / 获取订单信息").WithTags("PO");

            g.MapPut("Recalculate/{id:long}", (IPOService service, long id, CancellationToken cancellationToken) => service.RecalculateAsync(id, true, cancellationToken))
                .WithDescription("Recalculate PO / 重新计算订单").WithTags("PO");

            g.MapPut("Update", (IPOService service, POUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update PO / 更新订单").WithTags("PO");

            g.MapGet("UpdateRead/{id:long}", (IPOService service, long id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Get PO update info / 获取订单更新信息").WithTags("PO");

            return builder;
        }
    }
}

