using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Admin clear user frozen time message
    /// 管理员清除用户冻结时间消息
    /// </summary>
    public record AdminClearUserFrozenMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AdminClearUserFrozen";

        /// <summary>
        /// Frozen time
        /// 冻结时间
        /// </summary>
        public required DateTimeOffset FrozenTime { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(FrozenTime)] = FrozenTime
        };
    }
}
