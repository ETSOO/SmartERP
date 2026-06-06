using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Complete order line message processor
    /// 完成订单行消息处理器
    /// </summary>
    public class CompleteOrderLineProcessor : LogQueueProcessor<CompleteOrderLineMessage>
    {
        public CompleteOrderLineProcessor(ILogger<CompleteOrderLineProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CompleteOrderLineMessage, logDb)
        {
        }
    }
}
