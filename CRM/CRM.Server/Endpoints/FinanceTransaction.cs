using CRM.Server.RQ.FinanceAccount;
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

            g.MapPost("List", (IFinanceTransactionService service, FinanceTransactionListRQ rq, CancellationToken cancellationToken) => service.ListAsync(rq, cancellationToken))
                .WithDescription("List finance transactions / 列出财务交易").WithTags("FinanceTransaction");

            g.MapPost("Offset", (IFinanceTransactionService service, FinanceTransactionOffsetRQ rq, CancellationToken cancellationToken) => service.OffsetAsync(rq, cancellationToken))
                .WithDescription("Offset finance transaction / 冲减财务交易").WithTags("FinanceTransaction");

            g.MapPost("PayOrder", (IFinanceTransactionService service, FinanceTransactionPayOrderRQ rq, CancellationToken cancellationToken) => service.PayOrderAsync(rq, cancellationToken))
                .WithDescription("Pay order / 支付订单").WithTags("FinanceTransaction");

            g.MapPost("PayPO", (IFinanceTransactionService service, FinanceTransactionPayPORQ rq, CancellationToken cancellationToken) => service.PayPOAsync(rq, cancellationToken))
                .WithDescription("Pay purchase order / 支付采购订单").WithTags("FinanceTransaction");

            g.MapPost("Query", (IFinanceTransactionService service, FinanceTransactionQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query finance transactions / 查询财务交易").WithTags("FinanceTransaction");

            g.MapGet("Read/{id:long}", (IFinanceTransactionService service, long id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read finance transaction / 读取财务交易").WithTags("FinanceTransaction");

            g.MapPost("Reimburse", (IFinanceTransactionService service, FinanceTransactionReimburseRQ rq, CancellationToken cancellationToken) => service.ReimburseAsync(rq, cancellationToken))
                .WithDescription("Reimburse / 报销").WithTags("FinanceTransaction");

            g.MapPost("Transfer", (IFinanceTransactionService service, FinanceTransactionTransferRQ rq, CancellationToken cancellationToken) => service.TransferAsync(rq, cancellationToken))
                .WithDescription("Transfer / 转账").WithTags("FinanceTransaction");

            g.MapGet("ViewOrder/{id:long}", (IFinanceTransactionService service, long id, CancellationToken cancellationToken) => service.ViewOrderAsync(id, cancellationToken))
                .WithDescription("View order for collection / 浏览订单用于收款").WithTags("FinanceTransaction");

            g.MapGet("ViewPO/{id:long}", (IFinanceTransactionService service, long id, CancellationToken cancellationToken) => service.ViewPOAsync(id, cancellationToken))
                .WithDescription("View purchase order for payment / 浏览采购订单用于付款").WithTags("FinanceTransaction");

            return builder;
        }
    }
}
