using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Org
{
    /// <summary>
    /// Create asset message
    /// 创建资产消息
    /// </summary>
    public record CreateAssetMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateAsset";
    }
}
