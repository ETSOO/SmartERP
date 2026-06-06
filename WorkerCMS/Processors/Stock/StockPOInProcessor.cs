using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Stock PO in message processor
    /// 采购入库消息处理器
    /// </summary>
    public class StockPOInProcessor : LogQueueProcessor<StockPOInMessage>
    {
        public StockPOInProcessor(ILogger<StockPOInProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.StockPOInMessage, logDb)
        {
        }
    }
}
