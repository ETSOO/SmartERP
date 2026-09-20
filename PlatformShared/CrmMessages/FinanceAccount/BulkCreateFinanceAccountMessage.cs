using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.FinanceAccount
{
    /// <summary>
    /// Bulk create finance account message
    /// 批量创建财务账户消息
    /// </summary>
    public record BulkCreateFinanceAccountMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "BulkCreateFinanceAccount";
    }
}
