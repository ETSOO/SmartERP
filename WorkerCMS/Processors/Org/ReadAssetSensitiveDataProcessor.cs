using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Org
{
    /// <summary>
    /// Read asset sensitive data message processor
    /// 读取资产敏感数据消息处理器
    /// </summary>
    public class ReadAssetSensitiveDataProcessor : LogQueueProcessor<ReadAssetSensitiveDataMessage>
    {
        public ReadAssetSensitiveDataProcessor(ILogger<ReadAssetSensitiveDataProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.ReadAssetSensitiveDataMessage, logDb)
        {
        }
    }
}
