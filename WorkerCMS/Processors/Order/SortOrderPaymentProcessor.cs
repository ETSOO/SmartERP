using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Sort order payment message processor
    /// 排序订单支付方式消息处理器
    /// </summary>
    public class SortOrderPaymentProcessor : LogQueueProcessor<SortOrderPaymentMessage>
    {
        public SortOrderPaymentProcessor(ILogger<SortOrderPaymentProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.SortOrderPaymentMessage, logDbFactory)
        {
        }
    }
}
