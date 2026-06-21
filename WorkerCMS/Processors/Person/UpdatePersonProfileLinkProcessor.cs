using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Update person profile link message processor
    /// 更新个人资料链接消息处理器
    /// </summary>
    public class UpdatePersonProfileLinkProcessor : LogQueueProcessor<UpdatePersonProfileLinkMessage>
    {
        public UpdatePersonProfileLinkProcessor(ILogger<UpdatePersonProfileLinkProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.UpdatePersonProfileLinkMessage, logDbFactory)
        {
        }
    }
}
