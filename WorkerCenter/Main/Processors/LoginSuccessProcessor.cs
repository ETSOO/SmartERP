using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Login success processor
    /// 成功登录处理器
    /// </summary>
    public class LoginSuccessProcessor : LogQueueProcessor<LoginSuccessMessage>
    {
        public LoginSuccessProcessor(ILogger<LoginSuccessProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.LoginSuccessMessage, logDb)
        {
        }
    }
}
