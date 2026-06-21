using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Stock order out message processor
    /// 订单发货消息处理器
    /// </summary>
    public class StockOrderOutProcessor : LogQueueProcessor<StockOrderOutMessage>
    {
        public StockOrderOutProcessor(ILogger<StockOrderOutProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.StockOrderOutMessage, logDbFactory)
        {
        }
    }
}
