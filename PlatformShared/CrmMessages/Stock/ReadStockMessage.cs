using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Read stock message
    /// 读取库存消息
    /// </summary>
    public record ReadStockMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ReadStock";
    }
}
