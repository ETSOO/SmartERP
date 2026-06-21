using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Update stock message processor
    /// 更新库存消息处理器
    /// </summary>
    public class UpdateStockProcessor : LogQueueProcessor<UpdateStockMessage>
    {
        public UpdateStockProcessor(ILogger<UpdateStockProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateStockMessage, logDbFactory)
        {
        }
    }
}
