using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Create person product message
    /// 创建人员个性化产品消息
    /// </summary>
    public record CreatePersonProductMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreatePersonProduct";
    }
}
