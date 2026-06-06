using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Stock take message processor
    /// 库存盘点消息处理器
    /// </summary>
    public class StockTakeProcessor : LogQueueProcessor<StockTakeMessage>
    {
        public StockTakeProcessor(ILogger<StockTakeProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.StockTakeMessage, logDb)
        {
        }
    }
}
