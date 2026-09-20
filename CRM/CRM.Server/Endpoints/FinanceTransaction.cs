using CRM.Server.RQ.FinanceTransaction;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Finance transaction service APIs
    /// 财务交易服务API
    /// </summary>
    internal static class FinanceTransaction
    {
        public static RouteGroupBuilder MapFinanceTransaction(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("FinanceTransaction");

            g.MapPost("Adjust", (IFinanceTransactionService service, FinanceTransactionAdjustRQ rq, CancellationToken cancellationToken) => service.AdjustAsync(rq, cancellationToken))
                .WithDescription("Adjust finance transaction / 调整财务交易").WithTags("FinanceTransaction");

            g.MapPost("Offset", (IFinanceTransactionService service, FinanceTransactionOffsetRQ rq, CancellationToken cancellationToken) => service.OffsetAsync(rq, cancellationToken))
                .WithDescription("Offset finance transaction / 冲减财务交易").WithTags("FinanceTransaction");

            g.MapPost("PayOrder", (IFinanceTransactionService service, FinanceTransactionPayOrderRQ rq, CancellationToken cancellationToken) => service.PayOrderAsync(rq, cancellationToken))
                .WithDescription("Pay order / 支付订单").WithTags("FinanceTransaction");

            return builder;
        }
    }
}
