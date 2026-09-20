using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Org
{
    /// <summary>
    /// Delete asset message
    /// 移除资产消息
    /// </summary>
    public record DeleteAssetMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeleteAsset";
    }
}
