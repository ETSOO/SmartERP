using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Update person category message
    /// 更新人员类别消息
    /// </summary>
    public record UpdatePersonCategoryMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdatePersonCategory";
    }
}
