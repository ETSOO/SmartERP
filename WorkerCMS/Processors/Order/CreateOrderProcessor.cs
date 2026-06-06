using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Create order message processor
    /// 创建订单消息处理器
    /// </summary>
    public class CreateOrderProcessor : LogQueueProcessor<CreateOrderMessage>
    {
        public CreateOrderProcessor(ILogger<CreateOrderProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreateOrderMessage, logDb)
        {
        }
    }
}
