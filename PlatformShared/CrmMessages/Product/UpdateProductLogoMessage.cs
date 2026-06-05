using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Update product logo message
    /// 更新产品图标消息
    /// </summary>
    public record UpdateProductLogoMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateProductLogo";
    }
}
