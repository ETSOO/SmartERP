using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;

namespace WorkerCMS.Processors.PO
{
    /// <summary>
    /// Update purchase order line message processor
    /// 更新采购订单行消息处理器
    /// </summary>
    public class UpdatePOLineProcessor : LogQueueProcessor<UpdatePOLineMessage>
    {
        public UpdatePOLineProcessor(ILogger<UpdatePOLineProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdatePOLineMessage, logDbFactory)
        {
        }
    }
}
