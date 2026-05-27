using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
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

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(AppId)] = AppId,
            [nameof(FromOrganizationId)] = FromOrganizationId
        };
    }
}
