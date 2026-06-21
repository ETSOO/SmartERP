using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Messages;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Genereate document message processor
    /// 输出业务文档消息处理器
    /// </summary>
    public class GenerateDocumentProcessor : LogQueueProcessor<GenerateDocumentMessage>
    {
        public GenerateDocumentProcessor(ILogger<GenerateDocumentProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, PlatformSharedContext.Default.GenerateDocumentMessage, logDbFactory)
        {
        }
    }
}
