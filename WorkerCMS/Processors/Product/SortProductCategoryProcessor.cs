using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Sort product category message processor
    /// 排序产品类别消息处理器
    /// </summary>
    public class SortProductCategoryProcessor : LogQueueProcessor<SortProductCategoryMessage>
    {
        public SortProductCategoryProcessor(ILogger<SortProductCategoryProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.SortProductCategoryMessage, logDbFactory)
        {
        }
    }
}
