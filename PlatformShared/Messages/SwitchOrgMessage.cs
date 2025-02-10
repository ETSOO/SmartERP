using com.etsoo.MessageQueue;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record SwitchOrgMessageData
    {
        public required int AppId { get; init; }
        public int? FromOrganizationId { get; init; }
    }

    /// <summary>
    /// Switch org message
    /// 机构切换消息
    /// </summary>
    public record SwitchOrgMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "SwitchOrg";

        /// <summary>
        /// App ID
        /// 程序编号
        /// </summary>
        public required int AppId { get; init; }

        /// <summary>
        /// From organization ID
        /// 来自机构的编号
        /// </summary>
        public int? FromOrganizationId { get; init; }

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new SwitchOrgMessageData
            {
                AppId = AppId,
                FromOrganizationId = FromOrganizationId
            }, PlatformSharedContext.Default.SwitchOrgMessageData);
        }
    }
}
