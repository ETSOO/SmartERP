using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Org
{
    /// <summary>
    /// Delete asset message processor
    /// 移除资产消息处理器
    /// </summary>
    public class DeleteAssetProcessor : LogQueueProcessor<DeleteAssetMessage>
    {
        public DeleteAssetProcessor(ILogger<DeleteAssetProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.DeleteAssetMessage, logDbFactory)
        {
        }
    }
}
