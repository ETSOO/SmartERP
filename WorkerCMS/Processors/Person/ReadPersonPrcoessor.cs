using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    /// <summary>
    /// Read person message processor
    /// 读取人员信息处理器
    /// </summary>
    public class ReadPersonProcessor : LogQueueProcessor<ReadPersonMessage>
    {
        public ReadPersonProcessor(ILogger<ReadPersonProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.ReadPersonMessage, logDb)
        {
        }
    }
}
