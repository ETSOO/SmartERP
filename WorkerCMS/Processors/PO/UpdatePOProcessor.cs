using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Update purchase order message processor
    /// 更新采购订单消息处理器
    /// </summary>
    public class UpdatePOProcessor : LogQueueProcessor<UpdatePOMessage>
    {
        public UpdatePOProcessor(ILogger<UpdatePOProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdatePOMessage, logDbFactory)
        {
        }
    }
}
