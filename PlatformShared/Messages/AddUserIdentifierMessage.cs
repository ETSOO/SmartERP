using com.etsoo.Utils.Serialization;
using PlatformShared.Database.Models;

namespace PlatformShared.Messages
{
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

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            ["IdentifierType"] = IdentifierType.ToString(),
            ["IdentifierValue"] = IdentifierValue
        };
    }
}
