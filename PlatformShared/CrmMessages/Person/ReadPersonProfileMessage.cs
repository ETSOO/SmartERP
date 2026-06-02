using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Read person profile message
    /// 浏览人员档案消息
    /// </summary>
    public record ReadPersonProfileMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ReadPersonProfile";

        /// <summary>
        /// Is inner view
        /// 是否列表浏览
        /// </summary>
        public bool IsInner { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(IsInner)] = IsInner
        };
    }
}
