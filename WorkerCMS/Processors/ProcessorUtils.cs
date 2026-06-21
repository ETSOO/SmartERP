using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;

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
        /// <returns>Tuple containing order monthly report enabled flag and order daily report hour</returns>
        public static async Task<(bool OrderMonthlyReportEnabled, short? OrderDailyReportHour)> ReadReportSettingsAsync(MyDbContext db, int orgId, CancellationToken cancellationToken = default)
        {
            var settings = await db.SettingCrms.AsNoTracking()
                .Where(s => s.Id == orgId)
                .Select(s => new
                {
                    s.OrderMonthlyReportEnabled,
                    s.OrderDailyReportHour
                })
                .FirstOrDefaultAsync(cancellationToken);

            return (settings?.OrderMonthlyReportEnabled ?? false, settings?.OrderDailyReportHour);
        }
    }
}
