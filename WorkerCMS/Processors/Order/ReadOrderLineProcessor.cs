using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Read order line message processor
    /// 读取订单行消息处理器
    /// </summary>
    public class ReadOrderLineProcessor : LogQueueProcessor<ReadOrderLineMessage>
    {
        public ReadOrderLineProcessor(ILogger<ReadOrderLineProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.ReadOrderLineMessage, logDb)
        {
        }
    }
}
