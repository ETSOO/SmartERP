using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Update product unit message
    /// 更新产品单位消息
    /// </summary>
    public record UpdateProductUnitMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateProductUnit";
    }
}
