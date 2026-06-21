using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Stock lose message processor
    /// 库存报损消息处理器
    /// </summary>
    public class StockLoseProcessor : LogQueueProcessor<StockLoseMessage>
    {
        public StockLoseProcessor(ILogger<StockLoseProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.StockLoseMessage, logDbFactory)
        {
        }
    }
}
