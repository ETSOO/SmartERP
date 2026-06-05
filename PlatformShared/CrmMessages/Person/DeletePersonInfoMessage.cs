using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Delete person info message
    /// 移除人员信息消息
    /// </summary>
    public record DeletePersonInfoMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeletePersonInfo";
    }
}
