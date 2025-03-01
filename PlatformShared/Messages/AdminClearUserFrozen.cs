using com.etsoo.MessageQueue;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record AdminClearUserFrozenMessageData
    {
        public required DateTimeOffset FrozenTime { get; init; }
    }

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

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new AdminClearUserFrozenMessageData
            {
                FrozenTime = FrozenTime
            }, PlatformSharedContext.Default.AdminClearUserFrozenMessageData);
        }
    }
}
