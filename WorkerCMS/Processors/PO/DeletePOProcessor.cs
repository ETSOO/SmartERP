using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Delete purchase order message processor
    /// 移除采购订单消息处理器
    /// </summary>
    public class DeletePOProcessor : LogQueueProcessor<DeletePOMessage>
    {
        public DeletePOProcessor(ILogger<DeletePOProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.DeletePOMessage, logDbFactory)
        {
        }
    }
}
