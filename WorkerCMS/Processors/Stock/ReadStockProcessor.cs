using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Stock
{
    /// <summary>
    /// Read stock message processor
    /// 读取库存消息处理器
    /// </summary>
    public class ReadStockProcessor : LogQueueProcessor<ReadStockMessage>
    {
        public ReadStockProcessor(ILogger<ReadStockProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.ReadStockMessage, logDbFactory)
        {
        }
    }
}
