using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Read order message processor
    /// 读取订单消息处理器
    /// </summary>
    public class ReadOrderProcessor : LogQueueProcessor<ReadOrderMessage>
    {
        public ReadOrderProcessor(ILogger<ReadOrderProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.ReadOrderMessage, logDbFactory)
        {
        }
    }
}
