using com.etsoo.MessageQueue;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Admin clear user frozen time message
    /// 管理员清除用户冻结时间消息
    /// </summary>
    public record AdminClearUserFrozenMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AdminClearUserFrozen";
    }
}
