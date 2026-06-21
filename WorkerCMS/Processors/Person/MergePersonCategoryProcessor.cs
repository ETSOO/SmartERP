using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Merge person category message processor
    /// 合并人员分类消息处理器
    /// </summary>
    public class MergePersonCategoryProcessor : LogQueueProcessor<MergePersonCategoryMessage>
    {
        public MergePersonCategoryProcessor(ILogger<MergePersonCategoryProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.MergePersonCategoryMessage, logDbFactory)
        {
        }
    }
}
