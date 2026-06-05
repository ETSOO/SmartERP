using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Org
{
    /// <summary>
    /// Update user message
    /// 更新用户消息
    /// </summary>
    public record UpdateUserMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateUser";
    }
}
