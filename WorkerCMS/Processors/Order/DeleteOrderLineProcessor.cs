using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Delete order line message processor
    /// 移除订单行消息处理器
    /// </summary>
    public class DeleteOrderLineProcessor : LogQueueProcessor<DeleteOrderLineMessage>
    {
        public DeleteOrderLineProcessor(ILogger<DeleteOrderLineProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.DeleteOrderLineMessage, logDb)
        {
        }
    }
}
