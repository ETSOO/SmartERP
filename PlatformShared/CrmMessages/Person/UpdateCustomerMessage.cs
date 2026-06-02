using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Update customer message
    /// 更新客户信息
    /// </summary>
    public record UpdateCustomerMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateCustomer";
    }
}
