using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Create person location message processor
    /// 创建人员位置消息处理器
    /// </summary>
    public class CreatePersonLocationProcessor : LogQueueProcessor<CreatePersonLocationMessage>
    {
        public CreatePersonLocationProcessor(ILogger<CreatePersonLocationProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreatePersonLocationMessage, logDb)
        {
        }
    }
}
