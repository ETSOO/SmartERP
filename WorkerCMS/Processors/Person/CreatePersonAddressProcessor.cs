using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Create person address message processor
    /// 创建人员地址消息处理器
    /// </summary>
    public class CreatePersonAddressProcessor : LogQueueProcessor<CreatePersonAddressMessage>
    {
        public CreatePersonAddressProcessor(ILogger<CreatePersonAddressProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.CreatePersonAddressMessage, logDbFactory)
        {
        }
    }
}
