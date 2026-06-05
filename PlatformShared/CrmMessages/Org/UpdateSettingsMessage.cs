using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Org
{
    /// <summary>
    /// Update settings message
    /// 更新设置消息
    /// </summary>
    public record UpdateSettingsMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateSettings";
    }
}
