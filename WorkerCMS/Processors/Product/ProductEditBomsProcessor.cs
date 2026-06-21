using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Product edit BOMS message processor
    /// 编辑产品BOMS消息处理器
    /// </summary>
    public class ProductEditBomsProcessor : LogQueueProcessor<ProductEditBomsMessage>
    {
        public ProductEditBomsProcessor(ILogger<ProductEditBomsProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.ProductEditBomsMessage, logDbFactory)
        {
        }
    }
}
