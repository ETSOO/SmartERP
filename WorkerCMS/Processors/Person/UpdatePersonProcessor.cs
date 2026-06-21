using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Update person message processor
    /// 更新人员信息处理器
    /// </summary>
    public class UpdatePersonProcessor : LogQueueProcessor<UpdatePersonMessage>
    {
        public UpdatePersonProcessor(ILogger<UpdatePersonProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdatePersonMessage, logDbFactory)
        {
        }
    }
}
