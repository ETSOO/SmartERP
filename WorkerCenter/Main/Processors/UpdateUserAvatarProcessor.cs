using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// User update avatar processor
    /// 用户更新头像处理器
    /// </summary>
    public class UpdateUserAvatarProcessor : LogQueueProcessor<UpdateUserAvatarMessage>
    {
        public UpdateUserAvatarProcessor(ILogger<UpdateUserAvatarProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, PlatformSharedContext.Default.UpdateUserAvatarMessage, logDbFactory)
        {
        }
    }
}
