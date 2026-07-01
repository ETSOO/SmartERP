using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Update organization company seal message
    /// 更新机构公司印章消息
    /// </summary>
    public record UpdateOrgCompanySealMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateOrgCompanySeal";
    }
}
