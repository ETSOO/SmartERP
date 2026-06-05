using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Update product unit message processor
    /// 更新产品单位消息处理器
    /// </summary>
    public class UpdateProductUnitProcessor : LogQueueProcessor<UpdateProductUnitMessage>
    {
        public UpdateProductUnitProcessor(ILogger<UpdateProductUnitProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.UpdateProductUnitMessage, logDb)
        {
        }
    }
}
