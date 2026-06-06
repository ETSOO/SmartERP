using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Create order delivery message processor
    /// 创建订单配送方式消息处理器
    /// </summary>
    public class CreateOrderDeliveryProcessor : LogQueueProcessor<CreateOrderDeliveryMessage>
    {
        public CreateOrderDeliveryProcessor(ILogger<CreateOrderDeliveryProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreateOrderDeliveryMessage, logDb)
        {
        }
    }
}
