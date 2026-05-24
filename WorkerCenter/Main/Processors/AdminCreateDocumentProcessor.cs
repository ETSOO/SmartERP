using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Admin create document processor
    /// 管理员创建文档处理器
    /// </summary>
    public class AdminCreateDocumentProcessor : LogQueueProcessor<AdminCreateDocumentMessage>
    {
        public AdminCreateDocumentProcessor(ILogger<AdminCreateDocumentProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.AdminCreateDocumentMessage, logDb)
        {
        }
    }
}
