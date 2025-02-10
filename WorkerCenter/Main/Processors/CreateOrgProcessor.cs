using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Create organization processor
    /// 创建机构处理器
    /// </summary>
    public class CreateOrgProcessor : LogQueueProcessor<CreateOrgMessage>
    {
        public CreateOrgProcessor(ILogger<CreateOrgProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.CreateOrgMessage, logDb)
        {
        }
    }
}
