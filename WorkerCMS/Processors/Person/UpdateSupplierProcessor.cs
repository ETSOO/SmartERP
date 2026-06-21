using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Update supplier message processor
    /// 更新供应商消息处理器
    /// </summary>
    public class UpdateSupplierProcessor : LogQueueProcessor<UpdateSupplierMessage>
    {
        public UpdateSupplierProcessor(ILogger<UpdateSupplierProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateSupplierMessage, logDbFactory)
        {
        }
    }
}
