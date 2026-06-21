using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Stock assemble message processor
    /// 库存组装消息处理器
    /// </summary>
    public class StockAssembleProcessor : LogQueueProcessor<StockAssembleMessage>
    {
        public StockAssembleProcessor(ILogger<StockAssembleProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.StockAssembleMessage, logDbFactory)
        {
        }
    }
}
