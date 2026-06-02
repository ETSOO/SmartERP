using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Delete person profile attachment message
    /// 删除人员档案附件消息
    /// </summary>
    public record DeletePersonProfileAttachmentMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeletePersonProfileAttachment";
    }
}
