using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Create person info message
    /// 创建人员信息消息
    /// </summary>
    public record CreatePersonInfoMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreatePersonInfo";
    }
}
