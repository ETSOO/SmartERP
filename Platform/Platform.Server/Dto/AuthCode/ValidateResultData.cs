using PlatformShared.Dto;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Platform.Server.Dto.AuthCode
{
    /// <summary>
    /// Validate result data
    /// 验证结果数据
    /// </summary>
    public record ValidateResultData
    {
        /// <summary>
        /// Openid
        /// 公开编号
        /// </summary>
        public required string OpenId { get; init; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public int? UserId { get; init; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTime Expiry { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; init; }

        /// <summary>
        /// Deserialize data
        /// 反序列化数据
        /// </summary>
        /// <typeparam name="D">Generic data type</typeparam>
        /// <param name="typeInfo">JSON type info</param>
        /// <returns>Result</returns>
        public D? DeserializeData<D>(JsonTypeInfo<D> typeInfo) where D : AuthCodeData
        {
            if (Data == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize(Data, typeInfo);
        }
    }
}
