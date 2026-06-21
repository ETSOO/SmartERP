using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Create order line message processor
    /// 创建订单行消息处理器
    /// </summary>
    public class CreateOrderLineProcessor : LogQueueProcessor<CreateOrderLineMessage>
    {
        public CreateOrderLineProcessor(ILogger<CreateOrderLineProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.CreateOrderLineMessage, logDbFactory)
        {
        }
    }
}
