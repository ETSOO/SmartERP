using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Org
{
    /// <summary>
    /// Update asset message
    /// 更新资产消息
    /// </summary>
    public record UpdateAssetMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateAsset";
    }
}
