using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Update member avatar processor
    /// 更新成员头像处理器
    /// </summary>
    public class UpdateMemberAvatarProcessor : LogQueueProcessor<UpdateMemberAvatarMessage>
    {
        public UpdateMemberAvatarProcessor(ILogger<UpdateMemberAvatarProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, PlatformSharedContext.Default.UpdateMemberAvatarMessage, logDbFactory)
        {
        }
    }
}
