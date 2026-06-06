using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Rollback purchase order line message processor
    /// 回滚采购订单行消息处理器
    /// </summary>
    public class RollbackPOLineProcessor : LogQueueProcessor<RollbackPOLineMessage>
    {
        public RollbackPOLineProcessor(ILogger<RollbackPOLineProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.RollbackPOLineMessage, logDb)
        {
        }
    }
}
