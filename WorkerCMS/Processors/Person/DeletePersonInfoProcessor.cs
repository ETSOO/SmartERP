using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Delete person info message processor
    /// 移除人员信息消息处理器
    /// </summary>
    public class DeletePersonInfoProcessor : LogQueueProcessor<DeletePersonInfoMessage>
    {
        public DeletePersonInfoProcessor(ILogger<DeletePersonInfoProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.DeletePersonInfoMessage, logDb)
        {
        }
    }
}
