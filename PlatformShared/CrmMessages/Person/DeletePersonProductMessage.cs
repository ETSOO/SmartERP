using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Delete person product message
    /// 删除人员个性化产品消息
    /// </summary>
    public record DeletePersonProductMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeletePersonProduct";
    }
}
