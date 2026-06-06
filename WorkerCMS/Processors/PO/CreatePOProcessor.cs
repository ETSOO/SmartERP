using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Create purchase order message processor
    /// 创建采购订单消息处理器
    /// </summary>
    public class CreatePOProcessor : LogQueueProcessor<CreatePOMessage>
    {
        public CreatePOProcessor(ILogger<CreatePOProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreatePOMessage, logDb)
        {
        }
    }
}
