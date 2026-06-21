using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Create promotion message processor
    /// 创建促销消息处理器
    /// </summary>
    public class CreatePromotionProcessor : LogQueueProcessor<CreatePromotionMessage>
    {
        public CreatePromotionProcessor(ILogger<CreatePromotionProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.CreatePromotionMessage, logDbFactory)
        {
        }
    }
}
