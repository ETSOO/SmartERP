using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Org
{
    /// <summary>
    /// Update dept message
    /// 更新部门消息
    /// </summary>
    public record UpdateDeptMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateDept";
    }
}
