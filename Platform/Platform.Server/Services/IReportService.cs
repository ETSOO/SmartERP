using Platform.Server.Dto.Report;
using Platform.Server.Endpoints.Report.RQ;

namespace Platform.Server.Services
{
    public interface IReportService
    {
        Task<IEnumerable<OrderDailyReportData>> OrderDailyReportAsync(OrderDailyReportRQ rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderDailyReportQueryData>> OrderDailyReportQueryAsync(OrderDailyReportQueryRQ rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderMonthlyReportData>> OrderMonthlyReportAsync(OrderMonthlyReportRQ rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderMonthlyReportQueryData>> OrderMonthlyReportQueryAsync(OrderMonthlyReportQueryRQ rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<PeriodReportData>> UsageReportAsync(OrgUsageReportRQ rq, CancellationToken cancellationToken = default);
    }
}