using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Update contact relation message processor
    /// 更新联系人关系消息处理器
    /// </summary>
    public class UpdateContactRelationProcessor : LogQueueProcessor<UpdateContactRelationMessage>
    {
        public UpdateContactRelationProcessor(ILogger<UpdateContactRelationProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.UpdateContactRelationMessage, logDb)
        {
        }
    }
}
