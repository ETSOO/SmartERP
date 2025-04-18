using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Update organization message
    /// 更新机构消息
    /// </summary>
    public record UpdateOrgMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateOrg";
    }
}
