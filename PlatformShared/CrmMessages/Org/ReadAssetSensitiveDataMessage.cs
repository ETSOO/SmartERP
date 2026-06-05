using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Org
{
    /// <summary>
    /// Read asset sensitive data message
    /// 读取资产敏感数据消息
    /// </summary>
    public record ReadAssetSensitiveDataMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ReadAssetSensitiveData";
    }
}
