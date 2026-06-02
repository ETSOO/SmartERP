using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Create person profile message processor
    /// 创建个人档案消息处理器
    /// </summary>
    public class CreatePersonProfileProcessor : LogQueueProcessor<CreatePersonProfileMessage>
    {
        public CreatePersonProfileProcessor(ILogger<CreatePersonProfileProcessor> logger, LogDbContext logDb)
    : base(logger, CrmJsonSerializerContext.Default.CreatePersonProfileMessage, logDb)
        {
        }
    }
}
