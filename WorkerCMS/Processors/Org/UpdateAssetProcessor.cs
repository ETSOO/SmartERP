using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Org
{
    /// <summary>
    /// Update asset message processor
    /// 更新资产消息处理器
    /// </summary>
    public class UpdateAssetProcessor : LogQueueProcessor<UpdateAssetMessage>
    {
        public UpdateAssetProcessor(ILogger<UpdateAssetProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdateAssetMessage, logDbFactory)
        {
        }
    }
}
