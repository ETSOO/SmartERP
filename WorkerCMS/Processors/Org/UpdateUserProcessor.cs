using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Org
{
    /// <summary>
    /// Update user message processor
    /// 更新用户消息处理器
    /// </summary>
    public class UpdateUserProcessor : LogQueueProcessor<UpdateUserMessage>
    {
        public UpdateUserProcessor(ILogger<UpdateUserProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateUserMessage, logDbFactory)
        {
        }
    }
}
