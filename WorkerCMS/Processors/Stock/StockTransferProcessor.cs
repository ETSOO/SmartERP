using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Stock transfer message processor
    /// 库存调货消息处理器
    /// </summary>
    public class StockTransferProcessor : LogQueueProcessor<StockTransferMessage>
    {
        public StockTransferProcessor(ILogger<StockTransferProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.StockTransferMessage, logDbFactory)
        {
        }
    }
}
