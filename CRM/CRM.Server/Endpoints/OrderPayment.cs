using com.etsoo.WebUtils;
using CRM.Server.RQ.OrderPayment;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Order payment service APIs
    /// 订单支付方式服务API
    /// </summary>
    internal static class OrderPayment
    {
        public static RouteGroupBuilder MapOrderPayment(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("OrderPayment");

            g.MapPut("Create", (IOrderPaymentService service, OrderPaymentCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create order payment / 创建订单支付方式").WithTags("OrderPayment");

            g.MapPost("List", (IOrderPaymentService service, OrderPaymentListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get order payment list / 获取订单支付方式列表").WithTags("OrderPayment");

            g.MapPost("Query", (IOrderPaymentService service, OrderPaymentQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query order payment info / 查询订单支付方式信息").WithTags("OrderPayment");

            g.MapPut("Sort", (IOrderPaymentService service, Dictionary<int, short> rq, CancellationToken cancellationToken) => service.SortAsync(rq, cancellationToken))
                .WithDescription("Sort order payments / 排序订单支付方式").WithTags("OrderPayment");

            g.MapPut("Update", (IOrderPaymentService service, OrderPaymentUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update order payment / 更新订单支付方式").WithTags("OrderPayment");

            g.MapGet("UpdateRead/{id:int}", (IOrderPaymentService service, int id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Read order payment data for update / 读取用于更新的订单支付方式数据").WithTags("OrderPayment");

            return builder;
        }
    }
}
