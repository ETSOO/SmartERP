using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Update person info message
    /// 更新人员信息消息
    /// </summary>
    public record UpdatePersonInfoMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdatePersonInfo";
    }
}
