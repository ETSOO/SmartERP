using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Update order message processor
    /// 更新订单消息处理器
    /// </summary>
    public class UpdateOrderProcessor : LogQueueProcessor<UpdateOrderMessage>
    {
        public UpdateOrderProcessor(ILogger<UpdateOrderProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateOrderMessage, logDbFactory)
        {
        }
    }
}
