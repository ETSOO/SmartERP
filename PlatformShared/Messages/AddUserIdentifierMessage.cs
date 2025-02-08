using com.etsoo.MessageQueue;
using PlatformShared.Database.Models;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record AddUserIdentifierMessageData
    {
        public required string IdentifierType { get; init; }
        public required string IdentifierValue { get; init; }
    }

    /// <summary>
    /// Add user identifier message
    /// 添加用户标识消息
    /// </summary>
    public record AddUserIdentifierMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AddUserIdentifier";

        /// <summary>
        /// Identifier type
        /// 编号类型
        /// </summary>
        public required CoreUserIdentifierType IdentifierType { get; init; }

        /// <summary>
        /// Identifier value
        /// 编号值
        /// </summary>
        public required string IdentifierValue { get; init; }

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new AddUserIdentifierMessageData
            {
                IdentifierType = IdentifierType.ToString(),
                IdentifierValue = IdentifierValue
            }, PlatformSharedContext.Default.AddUserIdentifierMessageData);
        }
    }
}
