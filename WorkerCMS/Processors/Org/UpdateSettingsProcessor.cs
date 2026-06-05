using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Org
{
    /// <summary>
    /// Update system settings message processor
    /// 更新系统设置消息处理器
    /// </summary>
    public class UpdateSettingsProcessor : LogQueueProcessor<UpdateSettingsMessage>
    {
        public UpdateSettingsProcessor(ILogger<UpdateSettingsProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.UpdateSettingsMessage, logDb)
        {
        }
    }
}
