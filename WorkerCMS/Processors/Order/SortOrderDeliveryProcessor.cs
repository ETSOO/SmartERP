using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Sort order delivery message processor
    /// 排序订单配送方式消息处理器
    /// </summary>
    public class SortOrderDeliveryProcessor : LogQueueProcessor<SortOrderDeliveryMessage>
    {
        public SortOrderDeliveryProcessor(ILogger<SortOrderDeliveryProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.SortOrderDeliveryMessage, logDbFactory)
        {
        }
    }
}
