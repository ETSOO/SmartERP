using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Utils;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Dto.Report;
using Platform.Server.Endpoints.Report.RQ;
using PlatformShared.Database;
using PlatformShared.Services;

namespace Platform.Server.Services
{
    /// <summary>
    /// Report service
    /// 报表服务
    /// </summary>
    public class ReportService : CommonUserService, IReportService
    {
        readonly LogDbContext _logDb;
        readonly ISmartERPCoordinator _erp;
        readonly IOrgService _orgService;

        public ReportService(
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<OrgService> logger,
            LogDbContext logDb,
            ISmartERPCoordinator erp,
            IOrgService orgService) : base(app, userAccessor.UserSafe, "report", logger)
        {
            _logDb = logDb;
            _erp = erp;
            _orgService = orgService;
        }

        private AppActionData CreateOrderReportAction(AppActionData action)
        {
            action.Action = ServiceConstants.ReportOrderAction;
            action.TargetId = User.Pid;

            return action;
        }

        /// <summary>
        /// Order daily report
        /// 订单日报表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<OrderDailyReportData>> OrderDailyReportAsync(OrderDailyReportRQ rq, CancellationToken cancellationToken = default)
        {
            var action = CreateOrderReportAction(rq.Action);

            // Validate the action
            var actionResult = await _erp.ValidateActionAsync(action, cancellationToken);
            if (!actionResult.Ok)
            {
                return [];
            }

            var orgId = User.OrganizationInt;

            var days = rq.Days ?? 30;
            if (days < 5 || days > 90) days = 5;

            var start = rq.StartDate ?? DateOnly.FromDateTime(DateTime.Now).AddDays(-days);
            var end = start.AddDays(days);

            return await _logDb.OrderDailyReports.AsNoTracking()
                .Where(r => r.OrganizationId == orgId && r.Period >= start && r.Period <= end)
                .Select(r => new OrderDailyReportData
                {
                    Period = r.Period,
                    Items = r.Items,
                    Amount = r.Amount,
                    Customers = r.Customers
                })
                .OrderBy(r => r.Period)
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Order daily report query
        /// 订单日报表查询
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<OrderDailyReportQueryData>> OrderDailyReportQueryAsync(OrderDailyReportQueryRQ rq, CancellationToken cancellationToken = default)
        {
            var action = CreateOrderReportAction(rq.Action);

            // Validate the action
            var actionResult = await _erp.ValidateActionAsync(action, cancellationToken);
            if (!actionResult.Ok)
            {
                return [];
            }

            var orgId = User.OrganizationInt;

            return await _logDb.OrderDailyReports.AsNoTracking()
                .Where(r => r.OrganizationId == orgId)
                .QueryEtsoo(rq, (r) => r.Id, null, (q) =>
                {
                    var startDate = rq.StartDate;
                    if (startDate != null)
                    {
                        q = q.Where(r => r.Period >= startDate);
                    }

                    var endDate = rq.EndDate;
                    if (endDate != null)
                    {
                        q = q.Where(r => r.Period <= endDate);
                    }

                    return q;
                })
                .Select(t => new OrderDailyReportQueryData
                {
                    Id = t.Id,
                    Period = t.Period,
                    Items = t.Items,
                    Amount = t.Amount,
                    PaidAmount = t.PaidAmount,
                    Discount = t.Discount,
                    LineDiscount = t.LineDiscount,
                    ApprovedDiscount = t.ApprovedDiscount,
                    Qty = t.Qty,
                    Customers = t.Customers
                })
                .ToArrayAsync(cancellationToken);
        }

        private Task<List<OrderMonthlyReportData>> OrderMonthlyReportLoadAsync(int orgId, int year, CancellationToken cancellationToken = default)
        {
            var (start, end) = NumUtils.GetMonthPeriodRange(year);

            return _logDb.OrderMonthlyReports.AsNoTracking()
                .Where(r => r.OrganizationId == orgId && r.Period >= start && r.Period <= end)
                .Select(r => new OrderMonthlyReportData
                {
                    Period = r.Period,
                    Items = r.Items,
                    Amount = r.Amount,
                    Customers = r.Customers
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Order monthly report
        /// 订单月报表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<OrderMonthlyReportData>> OrderMonthlyReportAsync(OrderMonthlyReportRQ rq, CancellationToken cancellationToken = default)
        {
            var action = CreateOrderReportAction(rq.Action);

            // Validate the action
            var actionResult = await _erp.ValidateActionAsync(action, cancellationToken);
            if (!actionResult.Ok)
            {
                return [];
            }

            var orgId = User.OrganizationInt;

            var year = rq.Year ?? DateTime.UtcNow.Year;

            var hasLastYear = rq.HasLastYear ?? true;

            var data = await OrderMonthlyReportLoadAsync(orgId, year, cancellationToken);

            if (hasLastYear)
            {
                var lastYearData = await OrderMonthlyReportLoadAsync(orgId, year - 1, cancellationToken);
                data.AddRange(lastYearData);
            }

            return [.. data];
        }

        /// <summary>
        /// Order monthly report query
        /// 订单月报表查询
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<OrderMonthlyReportQueryData>> OrderMonthlyReportQueryAsync(OrderMonthlyReportQueryRQ rq, CancellationToken cancellationToken = default)
        {
            var action = CreateOrderReportAction(rq.Action);

            // Validate the action
            var actionResult = await _erp.ValidateActionAsync(action, cancellationToken);
            if (!actionResult.Ok)
            {
                return [];
            }

            var orgId = User.OrganizationInt;

            return await _logDb.OrderMonthlyReports.AsNoTracking()
                .Where(r => r.OrganizationId == orgId)
                .QueryEtsoo(rq, (r) => r.Id, null, (q) =>
                {
                    var startDate = rq.StartDate;
                    if (startDate != null)
                    {
                        var startPeriod = startDate.Value.Year * 100 + startDate.Value.Month;
                        q = q.Where(r => r.Period >= startPeriod);
                    }

                    var endDate = rq.EndDate;
                    if (endDate != null)
                    {
                        var endPeriod = endDate.Value.Year * 100 + endDate.Value.Month;
                        q = q.Where(r => r.Period <= endPeriod);
                    }

                    return q;
                })
                .Select(t => new OrderMonthlyReportQueryData
                {
                    Id = t.Id,
                    Period = t.Period,
                    Items = t.Items,
                    Amount = t.Amount,
                    PaidAmount = t.PaidAmount,
                    Discount = t.Discount,
                    LineDiscount = t.LineDiscount,
                    ApprovedDiscount = t.ApprovedDiscount,
                    Qty = t.Qty,
                    Customers = t.Customers
                })
                .ToArrayAsync(cancellationToken);
        }

        private Task<List<PeriodReportData>> UsageReportLoadAsync(int orgId, int year, CancellationToken cancellationToken)
        {
            var (start, end) = NumUtils.GetMonthPeriodRange(year);

            return _logDb.CoreLogUsages.AsNoTracking()
                .Where(u => u.OrganizationId == orgId && u.Period >= start && u.Period <= end)
                .Select(u => new PeriodReportData
                {
                    Period = u.Period,
                    Value = u.Qty
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get usage report data
        /// 获取使用报告数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<PeriodReportData>> UsageReportAsync(OrgUsageReportRQ rq, CancellationToken cancellationToken = default)
        {
            // Format request data
            var result = await _orgService.FormatRQAsync(rq, UserRole.User, cancellationToken);
            if (!result.Ok)
            {
                return [];
            }

            var orgId = rq.OrgId ?? User.OrganizationInt;

            var year = rq.Year ?? DateTime.UtcNow.Year;

            var hasLastYear = rq.HasLastYear ?? true;

            var data = await UsageReportLoadAsync(orgId, year, cancellationToken);

            if (hasLastYear)
            {
                var lastYearData = await UsageReportLoadAsync(orgId, year - 1, cancellationToken);
                data.AddRange(lastYearData);
            }

            return [.. data];
        }
    }
}
