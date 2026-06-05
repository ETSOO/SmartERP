using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Create person category message processor
    /// 创建人员分类消息处理器
    /// </summary>
    public class CreatePersonCategoryProcessor : LogQueueProcessor<CreatePersonCategoryMessage>
    {
        public CreatePersonCategoryProcessor(ILogger<CreatePersonCategoryProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreatePersonCategoryMessage, logDb)
        {
        }
    }
}
