using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Update person info message processor
    /// 更新人员信息消息处理器
    /// </summary>
    public class UpdatePersonInfoProcessor : LogQueueProcessor<UpdatePersonInfoMessage>
    {
        public UpdatePersonInfoProcessor(ILogger<UpdatePersonInfoProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.UpdatePersonInfoMessage, logDb)
        {
        }
    }
}
