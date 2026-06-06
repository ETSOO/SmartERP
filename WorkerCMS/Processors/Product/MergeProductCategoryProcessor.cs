using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Product
{
    /// <summary>
    /// Merge product category message processor
    /// 合并产品类别消息处理器
    /// </summary>
    public class MergeProductCategoryProcessor : LogQueueProcessor<MergeProductCategoryMessage>
    {
        public MergeProductCategoryProcessor(ILogger<MergeProductCategoryProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.MergeProductCategoryMessage, logDb)
        {
        }
    }
}
