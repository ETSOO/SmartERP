using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Delete person address message
    /// 删除人员地址消息
    /// </summary>
    public record DeletePersonAddressMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeletePersonAddress";
    }
}
