using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Recalculate order message processor
    /// 重新计算订单消息处理器
    /// </summary>
    public class RecalculateOrderProcessor : LogQueueProcessor<RecalculateOrderMessage>
    {
        public RecalculateOrderProcessor(ILogger<RecalculateOrderProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.RecalculateOrderMessage, logDb)
        {
        }
    }
}
