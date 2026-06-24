using Platform.Server.Endpoints.Report.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.Report
{
    /// <summary>
    /// Report service APIs
    /// 报表服务接口
    /// </summary>
    public static class Report
    {
        public static RouteGroupBuilder MapReport(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Report");

            g.MapPost("OrderDailyReport", (IReportService service, OrderDailyReportRQ rq, CancellationToken cancellationToken) => service.OrderDailyReportAsync(rq, cancellationToken))
                .WithDescription("Get order daily report / 获取订单日报表").WithTags("Report");

            g.MapPost("OrderDailyReportQuery", (IReportService service, OrderDailyReportQueryRQ rq, CancellationToken cancellationToken) => service.OrderDailyReportQueryAsync(rq, cancellationToken))
                .WithDescription("Query order daily report / 查询订单日报表").WithTags("Report");

            g.MapPost("OrderMonthlyReport", (IReportService service, OrderMonthlyReportRQ rq, CancellationToken cancellationToken) => service.OrderMonthlyReportAsync(rq, cancellationToken))
                .WithDescription("Get order monthly report / 获取订单月报表").WithTags("Report");

            g.MapPost("OrderMonthlyReportQuery", (IReportService service, OrderMonthlyReportQueryRQ rq, CancellationToken cancellationToken) => service.OrderMonthlyReportQueryAsync(rq, cancellationToken))
                .WithDescription("Query order monthly report / 查询订单月报表").WithTags("Report");

            g.MapPost("UsageReport", (IReportService service, OrgUsageReportRQ rq, CancellationToken cancellationToken) => service.UsageReportAsync(rq, cancellationToken))
                .WithDescription("Get organization usage / 获取机构使用情况").WithTags("Report");

            return builder;
        }
    }
}
