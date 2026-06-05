using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Create person product message processor
    /// 创建人员个性化产品消息处理器
    /// </summary>
    public class CreatePersonProductProcessor : LogQueueProcessor<CreatePersonProductMessage>
    {
        public CreatePersonProductProcessor(ILogger<CreatePersonProductProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreatePersonProductMessage, logDb)
        {
        }
    }
}
