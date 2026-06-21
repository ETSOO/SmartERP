using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Delete purchase order line message processor
    /// 移除采购订单行消息处理器
    /// </summary>
    public class DeletePOLineProcessor : LogQueueProcessor<DeletePOLineMessage>
    {
        public DeletePOLineProcessor(ILogger<DeletePOLineProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.DeletePOLineMessage, logDbFactory)
        {
        }
    }
}
