using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Org
{
    /// <summary>
    /// Create dept message processor
    /// 创建部门消息处理器
    /// </summary>
    public class CreateDeptProcessor : LogQueueProcessor<CreateDeptMessage>
    {
        public CreateDeptProcessor(ILogger<CreateDeptProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.CreateDeptMessage, logDb)
        {
        }
    }
}
