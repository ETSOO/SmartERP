using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.FinanceAccount;
using PlatformShared.Database;

namespace WorkerCMS.Processors.FinanceAccount
{
    /// <summary>
    /// Bulk create finance account message processor
    /// 批量创建财务账户消息处理器
    /// </summary>
    public class BulkCreateFinanceAccountProcessor : LogQueueProcessor<BulkCreateFinanceAccountMessage>
    {
        public BulkCreateFinanceAccountProcessor(ILogger<BulkCreateFinanceAccountProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.BulkCreateFinanceAccountMessage, logDbFactory)
        {
        }
    }
}
