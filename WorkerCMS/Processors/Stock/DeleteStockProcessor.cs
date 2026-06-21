using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Delete stock message processor
    /// 移除库存消息处理器
    /// </summary>
    public class DeleteStockProcessor : LogQueueProcessor<DeleteStockMessage>
    {
        public DeleteStockProcessor(ILogger<DeleteStockProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.DeleteStockMessage, logDbFactory)
        {
        }
    }
}
