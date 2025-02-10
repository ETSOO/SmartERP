using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Switch organization processor
    /// 切换机构处理器
    /// </summary>
    public class SwitchOrgProcessor : LogQueueProcessor<SwitchOrgMessage>
    {
        public SwitchOrgProcessor(ILogger<SwitchOrgProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.SwitchOrgMessage, logDb)
        {
        }
    }
}
