using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.FinanceAccount;
using PlatformShared.Database;

namespace WorkerCMS.Processors.FinanceAccount
{
    /// <summary>
    /// Create finance account message processor
    /// 创建财务账户消息处理器
    /// </summary>
    public class CreateFinanceAccountProcessor : LogQueueProcessor<CreateFinanceAccountMessage>
    {
        public CreateFinanceAccountProcessor(ILogger<CreateFinanceAccountProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.CreateFinanceAccountMessage, logDbFactory)
        {
        }
    }
}
