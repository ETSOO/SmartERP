using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Update person category message processor
    /// 更新人员分类消息处理器
    /// </summary>
    public class UpdatePersonCategoryProcessor : LogQueueProcessor<UpdatePersonCategoryMessage>
    {
        public UpdatePersonCategoryProcessor(ILogger<UpdatePersonCategoryProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.UpdatePersonCategoryMessage, logDb)
        {
        }
    }
}
