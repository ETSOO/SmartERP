using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Update organization processor
    /// 更新机构处理器
    /// </summary>
    public class UpdateOrgProcessor : LogQueueProcessor<UpdateOrgMessage>
    {
        public UpdateOrgProcessor(ILogger<UpdateOrgProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.UpdateOrgMessage, logDb)
        {
        }
    }
}
