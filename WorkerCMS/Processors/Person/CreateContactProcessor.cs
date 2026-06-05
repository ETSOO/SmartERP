using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Create contact message processor
    /// 创建联系人消息处理器
    /// </summary>
    public class CreateContactProcessor : LogQueueProcessor<CreateContactMessage>
    {
        public CreateContactProcessor(ILogger<CreateContactProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreateContactMessage, logDb)
        {
        }
    }
}
