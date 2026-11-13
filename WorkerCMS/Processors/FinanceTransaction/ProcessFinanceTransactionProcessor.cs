using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.FinanceTransaction;
using PlatformShared.Database;

namespace WorkerCMS.Processors.FinanceTransaction
{
    /// <summary>
    /// Process finance account message processor
    /// 处理财务账户消息处理器
    /// </summary>
    public class ProcessFinanceTransactionProcessor : LogQueueProcessor<ProcessFinanceTransactionMessage>
    {
        public ProcessFinanceTransactionProcessor(ILogger<ProcessFinanceTransactionProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.ProcessFinanceTransactionMessage, logDbFactory)
        {
        }
    }
}
