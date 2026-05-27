using com.etsoo.Utils.Serialization;
using PlatformShared.Database.Models;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Delete user identifier message
    /// 删除用户标识消息
    /// </summary>
    public record DeleteUserIdentifierMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeleteUserIdentifier";

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

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(IdentifierType)] = IdentifierType.ToString(),
            [nameof(IdentifierValue)] = IdentifierValue
        };
    }
}
