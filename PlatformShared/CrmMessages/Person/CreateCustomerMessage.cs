using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Create customer message
    /// 创建客户消息
    /// </summary>
    public record CreateCustomerMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateCustomer";
    }
}
