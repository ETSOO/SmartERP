using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Delete contact relation message processor
    /// 删除联系人关系消息处理器
    /// </summary>
    public class DeleteContactRelationProcessor : LogQueueProcessor<DeleteContactRelationMessage>
    {
        public DeleteContactRelationProcessor(ILogger<DeleteContactRelationProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.DeleteContactRelationMessage, logDb)
        {
        }
    }
}
