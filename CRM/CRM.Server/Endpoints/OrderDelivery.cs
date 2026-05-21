using com.etsoo.WebUtils;
using CRM.Server.RQ.OrderDelivery;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Order delivery service APIs
    /// 订单配送方式服务API
    /// </summary>
    internal static class OrderDelivery
    {
        public static RouteGroupBuilder MapOrderDelivery(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("OrderDelivery");

            g.MapPost("Create", (IOrderDeliveryService service, OrderDeliveryCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create order delivery / 创建订单配送方式").WithTags("OrderDelivery");

            g.MapPost("List", (IOrderDeliveryService service, OrderDeliveryListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get order delivery list / 获取订单配送方式列表").WithTags("OrderDelivery");

            g.MapPost("Query", (IOrderDeliveryService service, OrderDeliveryQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query order delivery info / 查询订单配送方式信息").WithTags("OrderDelivery");

            g.MapPut("Sort", (IOrderDeliveryService service, Dictionary<int, short> rq, CancellationToken cancellationToken) => service.SortAsync(rq, cancellationToken))
                .WithDescription("Sort order deliveries / 排序订单配送方式").WithTags("OrderDelivery");

            g.MapPut("Update", (IOrderDeliveryService service, OrderDeliveryUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update order delivery / 更新订单配送方式").WithTags("OrderDelivery");

            g.MapGet("UpdateRead/{id:int}", (IOrderDeliveryService service, int id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Read order delivery data for update / 读取用于更新的订单配送方式数据").WithTags("OrderDelivery");

            return builder;
        }
    }
}
