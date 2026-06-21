using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Update customer message processor
    /// 更新客户消息处理器
    /// </summary>
    public class UpdateCustomerProcessor : LogQueueProcessor<UpdateCustomerMessage>
    {
        public UpdateCustomerProcessor(ILogger<UpdateCustomerProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateCustomerMessage, logDbFactory)
        {
        }
    }
}
