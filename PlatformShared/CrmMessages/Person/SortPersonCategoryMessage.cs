using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Sort person category message
    /// 排序人员类别消息
    /// </summary>
    public record SortPersonCategoryMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "SortPersonCategory";
    }
}
