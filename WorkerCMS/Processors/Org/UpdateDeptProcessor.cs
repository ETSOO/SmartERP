using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Org
{
    /// <summary>
    /// Update dept message processor
    /// 更新部门消息处理器
    /// </summary>
    public class UpdateDeptProcessor : LogQueueProcessor<UpdateDeptMessage>
    {
        public UpdateDeptProcessor(ILogger<UpdateDeptProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.UpdateDeptMessage, logDb)
        {
        }
    }
}
