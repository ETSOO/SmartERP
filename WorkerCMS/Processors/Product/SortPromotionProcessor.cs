using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Sort promotion message processor
    /// 排序促销消息处理器
    /// </summary>
    public class SortPromotionProcessor : LogQueueProcessor<SortPromotionMessage>
    {
        public SortPromotionProcessor(ILogger<SortPromotionProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.SortPromotionMessage, logDbFactory)
        {
        }
    }
}
