using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Read purchase order message processor
    /// 读取采购订单消息处理器
    /// </summary>
    public class ReadPOProcessor : LogQueueProcessor<ReadPOMessage>
    {
        public ReadPOProcessor(ILogger<ReadPOProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.ReadPOMessage, logDbFactory)
        {
        }
    }
}
