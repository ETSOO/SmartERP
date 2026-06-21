using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Check app session processor
    /// 检查应用会话处理器
    /// </summary>
    public class CheckSessionProcessor : LogQueueProcessor<CheckSessionMessage>
    {
        public CheckSessionProcessor(ILogger<CheckSessionProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, PlatformSharedContext.Default.CheckSessionMessage, logDbFactory)
        {
        }
    }
}
