using com.etsoo.MessageQueue;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Create organization message
    /// 创建机构消息
    /// </summary>
    public record CreateOrgMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateOrg";
    }
}
