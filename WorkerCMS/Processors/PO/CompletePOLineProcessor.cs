using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Complete purchase order line message processor
    /// 完成采购订单行消息处理器
    /// </summary>
    public class CompletePOLineProcessor : LogQueueProcessor<CompletePOLineMessage>
    {
        public CompletePOLineProcessor(ILogger<CompletePOLineProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CompletePOLineMessage, logDb)
        {
        }
    }
}
