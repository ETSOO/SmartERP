using com.etsoo.WebUtils;
using CRM.Server.RQ.Order;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Order service APIs
    /// 订单服务API
    /// </summary>
    internal static class Order
    {
        public static RouteGroupBuilder MapOrder(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Order");

            g.MapPost("List", (IOrderService service, OrderListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get order list / 获取订单列表").WithTags("Order");

            g.MapPost("Query", (IOrderService service, OrderQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query order info / 查询订单信息").WithTags("Order");

            return builder;
        }
    }
}
