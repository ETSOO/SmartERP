using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Update person product message processor
    /// 更新人员个性化产品消息处理器
    /// </summary>
    public class UpdatePersonProductProcessor : LogQueueProcessor<UpdatePersonProductMessage>
    {
        public UpdatePersonProductProcessor(ILogger<UpdatePersonProductProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.UpdatePersonProductMessage, logDb)
        {
        }
    }
}
