using PlatformShared.Database.Models;

namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Check user identifier existance data
    /// 检查用户标识是否存在数据
    /// </summary>
    public record CheckUserIdentifierData
    {
        /// <summary>
        /// Type
        /// 类型
        /// </summary>
        public required CoreUserIdentifierType Type { get; init; }

        /// <summary>
        /// Openid
        /// 公开编号
        /// </summary>
        public required string Openid { get; init; }

        /// <summary>
        /// Region
        /// 所在地区
        /// </summary>
        public required string Region { get; init; }
    }
}
