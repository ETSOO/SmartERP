using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Org
{
    /// <summary>
    /// Create asset message processor
    /// 创建资产消息处理器
    /// </summary>
    public class CreateAssetProcessor : LogQueueProcessor<CreateAssetMessage>
    {
        public CreateAssetProcessor(ILogger<CreateAssetProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.CreateAssetMessage, logDbFactory)
        {
        }
    }
}
