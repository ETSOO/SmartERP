using com.etsoo.WebUtils;
using CRM.Server.RQ.Supplier;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Supplier service APIs
    /// 供应商服务API
    /// </summary>
    internal static class Supplier
    {
        public static RouteGroupBuilder MapSupplier(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Supplier");

            g.MapPost("List", (ISupplierService service, SupplierListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get supplier list / 获取供应商列表").WithTags("Supplier");

            g.MapPost("Query", (ISupplierService service, SupplierQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query supplier info / 查询供应商信息").WithTags("Supplier");

            return builder;
        }
    }
}
