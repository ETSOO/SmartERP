using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Update document processor
    /// 更新文档处理器
    /// </summary>
    public class UpdateDocumentProcessor : LogQueueProcessor<UpdateDocumentMessage>
    {
        public UpdateDocumentProcessor(ILogger<UpdateDocumentProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, PlatformSharedContext.Default.UpdateDocumentMessage, logDbFactory)
        {
        }
    }
}
