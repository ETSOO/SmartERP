using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Create document processor
    /// 创建文档处理器
    /// </summary>
    public class CreateDocumentProcessor : LogQueueProcessor<CreateDocumentMessage>
    {
        public CreateDocumentProcessor(ILogger<CreateDocumentProcessor> logger, LogDbContext logDb)
            : base(logger, PlatformSharedContext.Default.CreateDocumentMessage, logDb)
        {
        }
    }
}
