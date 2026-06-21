using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Update person profile message processor
    /// 更新个人资料消息处理器
    /// </summary>
    public class UpdatePersonProfileProcessor : LogQueueProcessor<UpdatePersonProfileMessage>
    {
        public UpdatePersonProfileProcessor(ILogger<UpdatePersonProfileProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdatePersonProfileMessage, logDbFactory)
        {
        }
    }
}
