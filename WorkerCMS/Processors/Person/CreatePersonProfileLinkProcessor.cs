using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Create person profile link message processor
    /// 创建个人档案链接消息处理器
    /// </summary>
    public class CreatePersonProfileLinkProcessor : LogQueueProcessor<CreatePersonProfileLinkMessage>
    {
        public CreatePersonProfileLinkProcessor(ILogger<CreatePersonProfileLinkProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreatePersonProfileLinkMessage, logDb)
        {
        }
    }
}
