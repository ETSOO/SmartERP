using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Update product logo message processor
    /// 更新产品图标消息处理器
    /// </summary>
    public class UpdateProductLogoProcessor : LogQueueProcessor<UpdateProductLogoMessage>
    {
        public UpdateProductLogoProcessor(ILogger<UpdateProductLogoProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.UpdateProductLogoMessage, logDb)
        {
        }
    }
}
