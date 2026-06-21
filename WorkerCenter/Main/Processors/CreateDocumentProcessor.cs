using Microsoft.EntityFrameworkCore;
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
        public CreateDocumentProcessor(ILogger<CreateDocumentProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, PlatformSharedContext.Default.CreateDocumentMessage, logDbFactory)
        {
        }
    }
}
