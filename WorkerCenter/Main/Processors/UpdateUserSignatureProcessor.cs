using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// User update signature message processor
    /// 用户更新签名消息处理器
    /// </summary>
    public class UpdateUserSignatureProcessor : LogQueueProcessor<UpdateUserSignatureMessage>
    {
        public UpdateUserSignatureProcessor(ILogger<UpdateUserSignatureProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, PlatformSharedContext.Default.UpdateUserSignatureMessage, logDbFactory)
        {
        }
    }
}
