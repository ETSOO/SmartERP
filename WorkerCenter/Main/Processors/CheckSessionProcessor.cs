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
        public CheckSessionProcessor(ILogger<CheckSessionProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.CheckSessionMessage, logDb)
        {
        }
    }
}
