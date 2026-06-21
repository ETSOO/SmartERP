using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Update user self data processor
    /// 更新用户本人信息处理器
    /// </summary>
    public class UpdateUserSelfProcessor : LogQueueProcessor<UpdateUserSelfMessage>
    {
        public UpdateUserSelfProcessor(ILogger<UpdateUserSelfProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, PlatformSharedContext.Default.UpdateUserSelfMessage, logDbFactory)
        {
        }
    }
}
