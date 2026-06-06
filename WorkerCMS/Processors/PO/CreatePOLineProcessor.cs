using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Create purchase order line message processor
    /// 创建采购订单行消息处理器
    /// </summary>
    public class CreatePOLineProcessor : LogQueueProcessor<CreatePOLineMessage>
    {
        public CreatePOLineProcessor(ILogger<CreatePOLineProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreatePOLineMessage, logDb)
        {
        }
    }
}
