using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Create contact message
    /// 创建联系人消息
    /// </summary>
    public record CreateContactMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateContact";
    }
}
