using com.etsoo.CoreFramework.Business;
using com.etsoo.Database.Converters;
using com.etsoo.Utils;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.LogDatabase.Models;

namespace WorkerCMS.Processors
{
    /// <summary>
    /// Processor utils
    /// 处理器工具
    /// </summary>
    public static class ProcessorUtils
    {
        /// <summary>
        /// Read report settings
        /// 读取报表设置
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="orgId">Organization ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple containing order monthly report enabled flag, order daily report hour, and timezone</returns>
        public static async Task<(bool OrderMonthlyReportEnabled, byte? OrderDailyReportHour, string? Timezone)> ReadReportSettingsAsync(MyDbContext db, int orgId, CancellationToken cancellationToken = default)
        {
            var settings = await db.SettingCrms.AsNoTracking()
                .Where(s => s.Id == orgId)
                .Select(s => new
                {
                    s.OrderMonthlyReportEnabled,
                    s.OrderDailyReportHour,
                    s.Organization.TimeZone
                })
                .FirstOrDefaultAsync(cancellationToken);

            return (settings?.OrderMonthlyReportEnabled ?? false, settings?.OrderDailyReportHour, settings?.TimeZone);
        }

        /// <summary>
        /// Create order daily report
        /// 创建订单日报表
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="logDb">Log database</param>
        /// <param name="orgId">Organization ID</param>
        /// <param name="hour">Hour of the day</param>
        /// <param name="now">Current time</param>
        /// <param name="timeZone">Time zone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async Task CreateOrderDailyReportAsync(MyDbContext db, LogDbContext logDb, int orgId, byte hour, DateTimeOffset now, string? timeZone, CancellationToken cancellationToken = default)
        {
            // Org's time zone
            var tz = TimeZoneUtils.GetTimeZone(timeZone);

            // Current time in org's time zone
            var orgNow = TimeZoneInfo.ConvertTime(now, tz);
            var date = orgNow.Date;
            var dateOnly = DateOnly.FromDateTime(date);

            // Start and end of the day in org's time zone
            var start = date.AddHours(hour).ToUniversalTime();
            var end = start.AddHours(24);

            var data = await db.Orders(orgId).AsNoTracking()
                .Where(o => o.Status < EntityStatus.Inactivated && o.Creation >= start && o.Creation < end)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Items = g.Count(),
                    Amount = g.Sum(o => o.Amount),
                    PaidAmount = g.Sum(o => o.PaidAmount),
                    Discount = g.Sum(o => o.Discount),
                    LineDiscount = g.Sum(o => o.LineDiscount),
                    ApprovedDiscount = g.Sum(o => o.ApprovedDiscount),
                    Qty = g.Sum(o => o.Items),
                    Customers = g.Select(o => o.BuyerId).Distinct().Count()
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Report data
            var report = await logDb.OrderDailyReports.Where(r => r.OrganizationId == orgId && r.Period == dateOnly).FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                if (report != null)
                {
                    logDb.OrderDailyReports.Remove(report);
                }
            }
            else
            {
                if (report == null)
                {
                    report = new OrderDailyReport
                    {
                        OrganizationId = orgId,
                        Period = dateOnly
                    };
                    logDb.OrderDailyReports.Add(report);
                }

                report.Items = data.Items;
                report.Amount = data.Amount;
                report.PaidAmount = data.PaidAmount;
                report.Discount = data.Discount;
                report.LineDiscount = data.LineDiscount;
                report.ApprovedDiscount = data.ApprovedDiscount;
                report.Qty = data.Qty;
                report.Customers = data.Customers;
            }

            await logDb.SaveChangesAsync(cancellationToken);

            // Monthly report depends on daily reports
            var mStart = start.AddDays(-start.Day + 1);
            var mEnd = mStart.AddMonths(1);

            var mCustomers = await db.Orders(orgId).AsNoTracking()
                .Where(o => o.Status < EntityStatus.Inactivated && o.Creation >= mStart && o.Creation < mEnd)
                .GroupBy(_ => 1)
                .Select(g => g.Select(o => o.BuyerId).Distinct().Count())
                .FirstOrDefaultAsync(cancellationToken);

            var oStart = DateOnly.FromDateTime(mStart);
            var oEnd = DateOnly.FromDateTime(mEnd);

            var mData = await logDb.OrderDailyReports.AsNoTracking()
                .Where(o => o.Period >= oStart && o.Period < oEnd)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Items = g.Sum(r => r.Items),
                    Amount = g.Sum(r => r.Amount),
                    PaidAmount = g.Sum(r => r.PaidAmount),
                    Discount = g.Sum(r => r.Discount),
                    LineDiscount = g.Sum(r => r.LineDiscount),
                    ApprovedDiscount = g.Sum(r => r.ApprovedDiscount),
                    Qty = g.Sum(r => r.Items)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var mPeriod = NumUtils.GetMonthPeriod(date);
            var mReport = await logDb.OrderMonthlyReports.Where(r => r.OrganizationId == orgId && r.Period == mPeriod).FirstOrDefaultAsync(cancellationToken);

            if (mData == null)
            {
                if (mReport != null)
                {
                    logDb.OrderMonthlyReports.Remove(mReport);
                }
            }
            else
            {
                if (mReport == null)
                {
                    mReport = new OrderMonthlyReport
                    {
                        OrganizationId = orgId,
                        Period = mPeriod
                    };
                    logDb.OrderMonthlyReports.Add(mReport);
                }

                mReport.Items = mData.Items;
                mReport.Amount = mData.Amount;
                mReport.PaidAmount = mData.PaidAmount;
                mReport.Discount = mData.Discount;
                mReport.LineDiscount = mData.LineDiscount;
                mReport.ApprovedDiscount = mData.ApprovedDiscount;
                mReport.Qty = mData.Qty;
                mReport.Customers = mCustomers;
            }

            await logDb.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Create order monthly report
        /// 创建订单月报表
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="logDb">Log database</param>
        /// <param name="orgId">Organization ID</param>
        /// <param name="hour">Hour of the day</param>
        /// <param name="now">Current time</param>
        /// <param name="timeZone">Time zone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async Task CreateOrderMonthlyReportAsync(MyDbContext db, LogDbContext logDb, int orgId, byte hour, DateTimeOffset now, string? timeZone, CancellationToken cancellationToken = default)
        {
            // Org's time zone
            var tz = TimeZoneUtils.GetTimeZone(timeZone);

            // Current time in org's time zone
            var orgNow = TimeZoneInfo.ConvertTime(now, tz);
            var date = new DateTime(orgNow.Year, orgNow.Month, 1);

            // Start and end of the month in org's time zone
            var start = date.AddHours(hour).ToUniversalTime();
            var end = start.AddMonths(1);

            var data = await db.Orders(orgId).AsNoTracking()
                .Where(o => o.Status < EntityStatus.Inactivated && o.Creation >= start && o.Creation < end)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Items = g.Count(),
                    Amount = g.Sum(o => o.Amount),
                    PaidAmount = g.Sum(o => o.PaidAmount),
                    Discount = g.Sum(o => o.Discount),
                    LineDiscount = g.Sum(o => o.LineDiscount),
                    ApprovedDiscount = g.Sum(o => o.ApprovedDiscount),
                    Qty = g.Sum(o => o.Items),
                    Customers = g.Select(o => o.BuyerId).Distinct().Count()
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Report data
            var period = NumUtils.GetMonthPeriod(date);
            var report = await logDb.OrderMonthlyReports.Where(r => r.OrganizationId == orgId && r.Period == period).FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                if (report != null)
                {
                    logDb.OrderMonthlyReports.Remove(report);
                }
            }
            else
            {
                if (report == null)
                {
                    report = new OrderMonthlyReport
                    {
                        OrganizationId = orgId,
                        Period = period
                    };
                    logDb.OrderMonthlyReports.Add(report);
                }

                report.Items = data.Items;
                report.Amount = data.Amount;
                report.PaidAmount = data.PaidAmount;
                report.Discount = data.Discount;
                report.LineDiscount = data.LineDiscount;
                report.ApprovedDiscount = data.ApprovedDiscount;
                report.Qty = data.Qty;
                report.Customers = data.Customers;
            }

            await logDb.SaveChangesAsync(cancellationToken);
        }
    }
}
