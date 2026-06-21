using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Read purchase order line message processor
    /// 读取采购订单行消息处理器
    /// </summary>
    public class ReadPOLineProcessor : LogQueueProcessor<ReadPOLineMessage>
    {
        public ReadPOLineProcessor(ILogger<ReadPOLineProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.ReadPOLineMessage, logDbFactory)
        {
        }
    }
}
