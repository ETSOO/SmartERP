using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Update promotion message processor
    /// 更新促销消息处理器
    /// </summary>
    public class UpdatePromotionProcessor : LogQueueProcessor<UpdatePromotionMessage>
    {
        public UpdatePromotionProcessor(ILogger<UpdatePromotionProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdatePromotionMessage, logDbFactory)
        {
        }
    }
}
