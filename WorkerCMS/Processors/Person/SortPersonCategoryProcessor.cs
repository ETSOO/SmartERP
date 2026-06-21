using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Sort person category message processor
    /// 排序人员分类消息处理器
    /// </summary>
    public class SortPersonCategoryProcessor : LogQueueProcessor<SortPersonCategoryMessage>
    {
        public SortPersonCategoryProcessor(ILogger<SortPersonCategoryProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.SortPersonCategoryMessage, logDbFactory)
        {
        }
    }
}
