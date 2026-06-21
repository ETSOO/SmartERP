using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Update product price message processor
    /// 更新产品价格消息处理器
    /// </summary>
    public class UpdateProductPriceProcessor : LogQueueProcessor<UpdateProductPriceMessage>
    {
        public UpdateProductPriceProcessor(ILogger<UpdateProductPriceProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateProductPriceMessage, logDbFactory)
        {
        }
    }
}
