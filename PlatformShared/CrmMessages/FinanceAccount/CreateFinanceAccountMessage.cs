using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.FinanceAccount
{
    /// <summary>
    /// Create finance account message
    /// 创建财务账户消息
    /// </summary>
    public record CreateFinanceAccountMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateFinanceAccount";
    }
}
