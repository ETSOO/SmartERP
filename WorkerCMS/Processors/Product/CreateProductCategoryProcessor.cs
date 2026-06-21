using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Create product category message processor
    /// 创建产品类别消息处理器
    /// </summary>
    public class CreateProductCategoryProcessor : LogQueueProcessor<CreateProductCategoryMessage>
    {
        public CreateProductCategoryProcessor(ILogger<CreateProductCategoryProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.CreateProductCategoryMessage, logDbFactory)
        {
        }
    }
}
