using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Org
{
    /// <summary>
    /// Update culture message
    /// 更新文化消息
    /// </summary>
    public record UpdateCultureMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateCulture";
    }
}
