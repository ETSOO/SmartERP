using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Delete person message processor
    /// 删除人员信息处理器
    /// </summary>
    public class DeletePersonProcessor : LogQueueProcessor<DeletePersonMessage>
    {
        public DeletePersonProcessor(ILogger<DeletePersonProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.DeletePersonMessage, logDb)
        {
        }
    }
}
