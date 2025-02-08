using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Update app processor
    /// 更新应用处理器
    /// </summary>
    public class UpdateAppProcessor : LogQueueProcessor<UpdateAppMessage>
    {
        public UpdateAppProcessor(ILogger<UpdateAppProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.UpdateAppMessage, logDb)
        {
        }
    }
}
