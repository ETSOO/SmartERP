using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.FinanceAccount
{
    /// <summary>
    /// Update finance account message
    /// 更新财务账户消息
    /// </summary>
    public record UpdateFinanceAccountMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateFinanceAccount";
    }
}
