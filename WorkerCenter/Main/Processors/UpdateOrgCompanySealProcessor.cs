using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Update organization company seal message processor
    /// 更新机构公司印章消息处理器
    /// </summary>
    public class UpdateOrgCompanySealProcessor : LogQueueProcessor<UpdateOrgCompanySealMessage>
    {
        public UpdateOrgCompanySealProcessor(ILogger<UpdateOrgCompanySealProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, PlatformSharedContext.Default.UpdateOrgCompanySealMessage, logDbFactory)
        {
        }
    }
}
