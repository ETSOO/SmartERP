using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Create order payment message processor
    /// 创建订单支付方式消息处理器
    /// </summary>
    public class CreateOrderPaymentProcessor : LogQueueProcessor<CreateOrderPaymentMessage>
    {
        public CreateOrderPaymentProcessor(ILogger<CreateOrderPaymentProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreateOrderPaymentMessage, logDb)
        {
        }
    }
}
