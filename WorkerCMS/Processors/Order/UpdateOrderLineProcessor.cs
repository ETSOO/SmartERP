using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Update order line message processor
    /// 更新订单行消息处理器
    /// </summary>
    public class UpdateOrderLineProcessor : LogQueueProcessor<UpdateOrderLineMessage>
    {
        public UpdateOrderLineProcessor(ILogger<UpdateOrderLineProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateOrderLineMessage, logDbFactory)
        {
        }
    }
}
