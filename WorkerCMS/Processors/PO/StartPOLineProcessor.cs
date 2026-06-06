using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Start purchase order line execution message processor
    /// 开始采购订单行执行消息处理器
    /// </summary>
    public class StartPOLineProcessor : LogQueueProcessor<StartPOLineMessage>
    {
        public StartPOLineProcessor(ILogger<StartPOLineProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.StartPOLineMessage, logDb)
        {
        }
    }
}
