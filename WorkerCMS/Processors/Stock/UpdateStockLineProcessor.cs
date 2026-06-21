using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Update stock line message processor
    /// 更新库存行消息处理器
    /// </summary>
    public class UpdateStockLineProcessor : LogQueueProcessor<UpdateStockLineMessage>
    {
        public UpdateStockLineProcessor(ILogger<UpdateStockLineProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateStockLineMessage, logDbFactory)
        {
        }
    }
}
