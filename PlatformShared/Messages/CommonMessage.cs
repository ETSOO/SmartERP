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
    }

    /// <summary>
    /// Common message
    /// 通用消息
    /// </summary>
    [JsonDerivedType(typeof(ChangePasswordMessage))]
    [JsonDerivedType(typeof(LoginFailedMessage))]
    [JsonDerivedType(typeof(LoginSuccessMessage))]

    public abstract record CommonMessage
    {
        /// <summary>
        /// Data
        /// 数据
        /// </summary>
        public required CommonMessageData Data { get; init; }

        /// <summary>
        /// Get more JSON data
        /// 获取更多JSON数据
        /// </summary>
        /// <returns>Result</returns>
        public virtual string? GetMoreData()
        {
            return null;
        }
    }
}
