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

            g.MapPost("Create", (IOrderService service, OrderCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create order / 创建订单").WithTags("Order");

            g.MapPost("DuplicateTest", (IOrderService service, OrderDuplicateTestRQ rq, CancellationToken cancellationToken) => service.DuplicateTestAsync(rq, cancellationToken))
                .WithDescription("Test for duplicate order or POs / 测试重复的订单或采购").WithTags("Order");

            g.MapPost("List", (IOrderService service, OrderListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get order list / 获取订单列表").WithTags("Order");

            g.MapPost("ListAll", (IOrderService service, OrderListAllRQ rq, CancellationToken cancellationToken) => service.ListAllAsync(rq, cancellationToken))
                .WithDescription("Get order / PO list / 获取订单/采购列表").WithTags("Order");

            g.MapPost("Query", (IOrderService service, OrderQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query order info / 查询订单信息").WithTags("Order");

            g.MapGet("Read/{id:long}", (IOrderService service, long id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Get order info / 获取订单信息").WithTags("Order");

            g.MapPut("Recalculate/{id:long}", (IOrderService service, long id, CancellationToken cancellationToken) => service.RecalculateAsync(id, true, cancellationToken))
                .WithDescription("Recalculate order / 重新计算订单").WithTags("Order");

            g.MapPut("Update", (IOrderService service, OrderUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update order / 更新订单").WithTags("Order");

            g.MapGet("UpdateRead/{id:long}", (IOrderService service, long id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Get order update info / 获取订单更新信息").WithTags("Order");

            return builder;
        }
    }
}
