using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Delete person product message processor
    /// 移除人员个性化产品消息处理器
    /// </summary>
    public class DeletePersonProductProcessor : LogQueueProcessor<DeletePersonProductMessage>
    {
        public DeletePersonProductProcessor(ILogger<DeletePersonProductProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.DeletePersonProductMessage, logDbFactory)
        {
        }
    }
}
