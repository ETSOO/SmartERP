using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Delete person profile attachment processor
    /// 删除个人档案附件处理器
    /// </summary>
    public class DeletePersonProfileAttachmentProcessor : LogQueueProcessor<DeletePersonProfileAttachmentMessage>
    {
        public DeletePersonProfileAttachmentProcessor(ILogger<DeletePersonProfileAttachmentProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.DeletePersonProfileAttachmentMessage, logDbFactory)
        {
        }
    }
}
