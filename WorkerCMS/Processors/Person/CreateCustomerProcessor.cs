using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Create customer message processor
    /// 创建客户消息处理器
    /// </summary>
    public class CreateCustomerProcessor : LogQueueProcessor<CreateCustomerMessage>
    {
        public CreateCustomerProcessor(ILogger<CreateCustomerProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreateCustomerMessage, logDb)
        {
        }
    }
}
