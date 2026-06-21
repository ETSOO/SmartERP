using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Add contact relation message processor
    /// 添加联系人关系消息处理器
    /// </summary>
    public class AddContactRelationProcessor : LogQueueProcessor<AddContactRelationMessage>
    {
        public AddContactRelationProcessor(ILogger<AddContactRelationProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.AddContactRelationMessage, logDbFactory)
        {
        }
    }
}
