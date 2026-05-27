using com.etsoo.Database;
using System.Text.Json;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Common update message data
    /// 通用更新消息数据
    /// </summary>
    public record CommonUpdateMessageData
    {
        /// <summary>
        /// Changes
        /// 更改
        /// </summary>
        public required IEnumerable<EntityChangedProperty> Changes { get; init; }
    }

    /// <summary>
    /// Common update message
    /// 通用更新消息
    /// </summary>
    public abstract record CommonUpdateMessage : CommonMessage
    {
        /// <summary>
        /// Changes
        /// 更改
        /// </summary>
        public required IEnumerable<EntityChangedProperty> Changes
        {
            get;
            init
            {
                field = value;

                JsonData = JsonSerializer.Serialize(new CommonUpdateMessageData
                {
                    Changes = value
                }, PlatformSharedContext.Default.CommonUpdateMessageData);
            }
        }
    }
}
