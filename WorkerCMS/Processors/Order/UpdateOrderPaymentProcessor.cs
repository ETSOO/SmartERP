using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Update order payment message processor
    /// 更新订单支付方式消息处理器
    /// </summary>
    public class UpdateOrderPaymentProcessor : LogQueueProcessor<UpdateOrderPaymentMessage>
    {
        public UpdateOrderPaymentProcessor(ILogger<UpdateOrderPaymentProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateOrderPaymentMessage, logDbFactory)
        {
        }
    }
}
