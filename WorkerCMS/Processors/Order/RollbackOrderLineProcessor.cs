using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Rollback order line message processor
    /// 回滚订单行消息处理器
    /// </summary>
    public class RollbackOrderLineProcessor : LogQueueProcessor<RollbackOrderLineMessage>
    {
        public RollbackOrderLineProcessor(ILogger<RollbackOrderLineProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.RollbackOrderLineMessage, logDb)
        {
        }
    }
}
