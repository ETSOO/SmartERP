using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Create supplier message processor
    /// 创建供应商消息处理器
    /// </summary>
    public class CreateSupplierProcessor : LogQueueProcessor<CreateSupplierMessage>
    {
        public CreateSupplierProcessor(ILogger<CreateSupplierProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.CreateSupplierMessage, logDbFactory)
        {
        }
    }
}
