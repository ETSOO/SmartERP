using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Stock init message processor
    /// 库存初始化消息处理器
    /// </summary>
    public class StockInitProcessor : LogQueueProcessor<StockInitMessage>
    {
        public StockInitProcessor(ILogger<StockInitProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.StockInitMessage, logDb)
        {
        }
    }
}
