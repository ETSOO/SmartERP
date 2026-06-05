using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Create person location message
    /// 创建人员位置消息
    /// </summary>
    public record CreatePersonLocationMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreatePersonLocation";
    }
}
