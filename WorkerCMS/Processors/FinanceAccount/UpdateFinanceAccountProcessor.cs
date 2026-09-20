using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.FinanceAccount;
using PlatformShared.Database;

namespace WorkerCMS.Processors.FinanceAccount
{
    /// <summary>
    /// Update finance account message processor
    /// 更新财务账号消息处理器
    /// </summary>
    public class UpdateFinanceAccountProcessor : LogQueueProcessor<UpdateFinanceAccountMessage>
    {
        public UpdateFinanceAccountProcessor(ILogger<UpdateFinanceAccountProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateFinanceAccountMessage, logDbFactory)
        {
        }
    }
}
