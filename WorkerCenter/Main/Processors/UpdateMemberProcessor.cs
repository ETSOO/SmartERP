using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Update member processor
    /// 更新成员处理器
    /// </summary>
    public class UpdateMemberProcessor : LogQueueProcessor<UpdateMemberMessage>
    {
        public UpdateMemberProcessor(ILogger<UpdateMemberProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.UpdateMemberMessage, logDb)
        {
        }
    }
}
