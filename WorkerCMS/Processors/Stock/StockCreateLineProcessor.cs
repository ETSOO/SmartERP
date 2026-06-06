using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Stock create line message processor
    /// 库存创建行消息处理器
    /// </summary>
    public class StockCreateLineProcessor : LogQueueProcessor<StockCreateLineMessage>
    {
        public StockCreateLineProcessor(ILogger<StockCreateLineProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.StockCreateLineMessage, logDb)
        {
        }
    }
}
