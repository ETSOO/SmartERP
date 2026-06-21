using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Org
{
    /// <summary>
    /// Update culture message processor
    /// 更新文化消息处理器
    /// </summary>
    public class UpdateCultureProcessor : LogQueueProcessor<UpdateCultureMessage>
    {
        public UpdateCultureProcessor(ILogger<UpdateCultureProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateCultureMessage, logDbFactory)
        {
        }
    }
}
