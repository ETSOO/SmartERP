using com.etsoo.Utils.Serialization;
using PlatformShared.Database.Models;
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

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FinanceTransactionKind Kind { get; init; }

        /// <summary>
        /// Is last transaction
        /// 是否为最后的交易
        /// </summary>
        public bool IsLast { get; init; }
    }
}
