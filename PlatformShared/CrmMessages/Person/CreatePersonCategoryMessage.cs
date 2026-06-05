using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Create person category message
    /// 创建人员类别消息
    /// </summary>
    public record CreatePersonCategoryMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreatePersonCategory";
    }
}
