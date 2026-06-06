using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Stock receive message processor
    /// 入库消息处理器
    /// </summary>
    public class StockReceiveProcessor : LogQueueProcessor<StockReceiveMessage>
    {
        public StockReceiveProcessor(ILogger<StockReceiveProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.StockReceiveMessage, logDb)
        {
        }
    }
}
