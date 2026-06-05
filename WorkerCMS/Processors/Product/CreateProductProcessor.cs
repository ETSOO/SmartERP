using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Create product message processor
    /// 创建产品消息处理器
    /// </summary>
    public class CreateProductProcessor : LogQueueProcessor<CreateProductMessage>
    {
        public CreateProductProcessor(ILogger<CreateProductProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreateProductMessage, logDb)
        {
        }
    }
}
