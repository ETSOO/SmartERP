using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Recalculate purchase order message processor
    /// 重新计算采购订单消息处理器
    /// </summary>
    public class RecalculatePOProcessor : LogQueueProcessor<RecalculatePOMessage>
    {
        public RecalculatePOProcessor(ILogger<RecalculatePOProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.RecalculatePOMessage, logDb)
        {
        }
    }
}
