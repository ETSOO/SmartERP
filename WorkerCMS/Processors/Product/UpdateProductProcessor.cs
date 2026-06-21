using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Update product message processor
    /// 更新产品消息处理器
    /// </summary>
    public class UpdateProductProcessor : LogQueueProcessor<UpdateProductMessage>
    {
        public UpdateProductProcessor(ILogger<UpdateProductProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateProductMessage, logDbFactory)
        {
        }
    }
}
