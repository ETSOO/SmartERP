using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Update organization avatar processor
    /// 更新机构头像处理器
    /// </summary>
    public class UpdateOrgAvatarProcessor : LogQueueProcessor<UpdateOrgAvatarMessage>
    {
        public UpdateOrgAvatarProcessor(ILogger<UpdateOrgAvatarProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.UpdateOrgAvatarMessage, logDb)
        {
        }
    }
}
