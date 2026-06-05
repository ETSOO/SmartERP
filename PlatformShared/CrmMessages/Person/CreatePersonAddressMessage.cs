using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Create person address message
    /// 创建人员地址消息
    /// </summary>
    public record CreatePersonAddressMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreatePersonAddress";
    }
}
