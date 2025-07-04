using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Create resource processor
    /// 创建资源处理器
    /// </summary>
    public class CreateResourceProcessor : LogQueueProcessor<CreateResourceMessage>
    {
        public CreateResourceProcessor(ILogger<CreateResourceProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.CreateResourceMessage, logDb)
        {
        }
    }
}
