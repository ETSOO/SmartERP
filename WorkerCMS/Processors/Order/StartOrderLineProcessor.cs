using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Start order line execution message processor
    /// 开始订单行执行消息处理器
    /// </summary>
    public class StartOrderLineProcessor : LogQueueProcessor<StartOrderLineMessage>
    {
        public StartOrderLineProcessor(ILogger<StartOrderLineProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.StartOrderLineMessage, logDbFactory)
        {
        }
    }
}
