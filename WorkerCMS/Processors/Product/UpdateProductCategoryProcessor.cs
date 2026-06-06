using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Update product category message processor
    /// 更新产品类别消息处理器
    /// </summary>
    public class UpdateProductCategoryProcessor : LogQueueProcessor<UpdateProductCategoryMessage>
    {
        public UpdateProductCategoryProcessor(ILogger<CreateProductCategoryProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.UpdateProductCategoryMessage, logDb)
        {
        }
    }
}
