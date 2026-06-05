using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Create person info message processor
    /// 创建人员信息消息处理器
    /// </summary>
    public class CreatePersonInfoProcessor : LogQueueProcessor<CreatePersonInfoMessage>
    {
        public CreatePersonInfoProcessor(ILogger<CreatePersonInfoProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreatePersonInfoMessage, logDb)
        {
        }
    }
}
