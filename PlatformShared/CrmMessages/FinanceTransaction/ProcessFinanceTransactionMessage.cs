using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.FinanceTransaction
{
    /// <summary>
    /// Process finance transaction message
    /// 处理财务交易消息
    /// </summary>
    public record ProcessFinanceTransactionMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ProcessFinanceTransaction";
    }
}
