using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Update order delivery message processor
    /// 更新订单配送方式消息处理器
    /// </summary>
    public class UpdateOrderDeliveryProcessor : LogQueueProcessor<UpdateOrderDeliveryMessage>
    {
        public UpdateOrderDeliveryProcessor(ILogger<UpdateOrderDeliveryProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateOrderDeliveryMessage, logDbFactory)
        {
        }
    }
}
