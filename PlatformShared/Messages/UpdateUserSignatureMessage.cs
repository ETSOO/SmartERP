using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// User update signature message
    /// 用户更新签名消息
    /// </summary>
    public record UpdateUserSignatureMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateUserSignature";
    }
}
