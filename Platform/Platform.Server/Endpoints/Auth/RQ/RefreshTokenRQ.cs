using com.etsoo.WebUtils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Platform.Server.Endpoints.Auth.RQ
{
    /// <summary>
    /// Refresh token request data
    /// 刷新令牌请求数据
    /// </summary>
    public record RefreshTokenRQ
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        [StringLength(512, MinimumLength = 32)]
        public required string DeviceId { get; init; }

        /// <summary>
        /// Login password
        /// 登录密码
        /// </summary>
        [StringLength(512, MinimumLength = 64)]
        public string? Pwd { get; init; }

        /// <summary>
        /// Country code, like CN = China
        /// 国家编号，如 CN = 中国
        /// </summary>
        [RegionId]
        public required string Region { get; init; }
    }
}
