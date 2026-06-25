using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Report message processor
    /// 报表消息处理器
    /// </summary>
    public class ReportProcessor : LogQueueProcessor<ReportMessage>
    {
        public ReportProcessor(ILogger<ReportProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, PlatformSharedContext.Default.ReportMessage, logDbFactory)
        {
        }
    }
}
