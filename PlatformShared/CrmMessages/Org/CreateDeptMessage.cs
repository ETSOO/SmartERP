using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Org
{
    /// <summary>
    /// Create dept message
    /// 创建部门消息
    /// </summary>
    public record CreateDeptMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateDept";
    }
}
