using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Delete product message processor
    /// 移除产品消息处理器
    /// </summary>
    public class DeleteProductProcessor : LogQueueProcessor<DeleteProductMessage>
    {
        public DeleteProductProcessor(ILogger<DeleteProductProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.DeleteProductMessage, logDb)
        {
        }
    }
}
