using System.Text.Json.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Common message data
    /// 通用消息数据
    /// </summary>
    public record CommonMessageData
    {
        /// <summary>
        /// Application ID
        /// 应用编号
        /// </summary>
        public required int AppId { get; init; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public required string Culture { get; init; }

        /// <summary>
        /// Device ID
        /// 设备编号
        /// </summary>
        public int? DeviceId { get; init; }

        /// <summary>
        /// IP address
        /// IP地址
        /// </summary>
        public required string IP { get; init; }

        /// <summary>
        /// User ID
        /// 用户编号
        /// </summary>
        public required int UserId { get; init; }

        /// <summary>
        /// User name
        /// 用户姓名
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Organization ID
        /// 机构编号
        /// </summary>
        public int? OrganizationId { get; init; }

        /// <summary>
        /// Time zone
        /// 时区
        /// </summary>
        public required string TimeZone { get; init; }

        /// <summary>
        /// Target id
        /// 目标编号
        /// </summary>
        public required long TargetId { get; init; }

        /// <summary>
        /// Target name
        /// 目标名称
        /// </summary>
        public string? TargetName { get; init; }

        /// <summary>
        /// Time stamp
        /// 时间戳
        /// </summary>
        public DateTimeOffset TimeStamp { get; init; } = DateTimeOffset.Now;
    }

    /// <summary>
    /// Common message
    /// 通用消息
    /// </summary>
    [JsonDerivedType(typeof(AcceptInvitationMessage))]
    [JsonDerivedType(typeof(AddUserIdentifierMessage))]
    [JsonDerivedType(typeof(AdjustReportToMessage))]
    [JsonDerivedType(typeof(AdminClearUserFrozenMessage))]
    [JsonDerivedType(typeof(AdminSupportMessage))]
    [JsonDerivedType(typeof(AdminRenewAppMessage))]
    [JsonDerivedType(typeof(BuyAppMessage))]
    [JsonDerivedType(typeof(ChangePasswordMessage))]
    [JsonDerivedType(typeof(CheckSessionMessage))]
    [JsonDerivedType(typeof(CreateApiMessage))]
    [JsonDerivedType(typeof(CreateApiKeyMessage))]
    [JsonDerivedType(typeof(CreateDocumentMessage))]
    [JsonDerivedType(typeof(CreateOrgMessage))]
    [JsonDerivedType(typeof(CreateResourceMessage))]
    [JsonDerivedType(typeof(DeleteDocumentMessage))]
    [JsonDerivedType(typeof(DeleteMemberMessage))]
    [JsonDerivedType(typeof(DeleteUserIdentifierMessage))]
    [JsonDerivedType(typeof(LeaveOrgMessage))]
    [JsonDerivedType(typeof(LoginFailedMessage))]
    [JsonDerivedType(typeof(LoginSuccessMessage))]
    [JsonDerivedType(typeof(RenewAppMessage))]
    [JsonDerivedType(typeof(ResetPasswordMessage))]
    [JsonDerivedType(typeof(SendAuthCodeEmailMessage))]
    [JsonDerivedType(typeof(SendProfileEmailMessage))]
    [JsonDerivedType(typeof(SwitchOrgMessage))]
    [JsonDerivedType(typeof(UpdateApiMessage))]
    [JsonDerivedType(typeof(UpdateAppMessage))]
    [JsonDerivedType(typeof(UpdateDocumentMessage))]
    [JsonDerivedType(typeof(UpdateMemberAvatarMessage))]
    [JsonDerivedType(typeof(UpdateMemberMessage))]
    [JsonDerivedType(typeof(UpdateOrgAvatarMessage))]
    [JsonDerivedType(typeof(UpdateOrgMessage))]
    [JsonDerivedType(typeof(UpdateUserAvatarMessage))]
    [JsonDerivedType(typeof(UpdateUserSelfMessage))]

    public abstract record CommonMessage
    {
        /// <summary>
        /// Data
        /// 数据
        /// </summary>
        public required CommonMessageData Data { get; init; }

        /// <summary>
        /// Json data for static log
        /// 用于静态日志的 JSON 数据
        /// </summary>
        public string? JsonData { get; init; }

        /// <summary>
        /// Get more JSON data
        /// 获取更多JSON数据
        /// </summary>
        /// <returns>Result</returns>
        public virtual Dictionary<string, object?>? GetJsonData()
        {
            return null;
        }
    }
}
