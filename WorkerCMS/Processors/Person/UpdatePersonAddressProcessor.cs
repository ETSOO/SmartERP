using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Update person address message processor
    /// 更新人员地址信息处理器
    /// </summary>
    public class UpdatePersonAddressProcessor : LogQueueProcessor<UpdatePersonAddressMessage>
    {
        public UpdatePersonAddressProcessor(ILogger<UpdatePersonAddressProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdatePersonAddressMessage, logDbFactory)
        {
        }
    }
}
