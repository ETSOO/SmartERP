using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Delete person profile link processor
    /// 删除个人档案链接处理器
    /// </summary>
    public class DeletePersonProfileLinkProcessor : LogQueueProcessor<DeletePersonProfileLinkMessage>
    {
        public DeletePersonProfileLinkProcessor(ILogger<DeletePersonProfileLinkProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.DeletePersonProfileLinkMessage, logDbFactory)
        {
        }
    }
}
