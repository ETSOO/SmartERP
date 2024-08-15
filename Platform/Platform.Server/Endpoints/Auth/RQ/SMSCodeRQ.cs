using com.etsoo.WebUtils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Platform.Server.Endpoints.Auth.RQ
{
    /// <summary>
    /// SMS code request data
    /// 短信验证码请求数据
    /// </summary>
    public record SMSCodeRQ
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        [StringLength(512, MinimumLength = 32)]
        public required string DeviceId { get; init; }

        /// <summary>
        /// Action
        /// 操作
        /// </summary>
        public required short Action { get; init; }

        /// <summary>
        /// User's mobile
        /// 用户手机号码
        /// </summary>
        [StringLength(512, MinimumLength = 64)]
        public required string Mobile { get; init; }

        /// <summary>
        /// Country or region code, like CN = China
        /// 国家或地区编号，如 CN = 中国
        /// </summary>
        [RegionId]
        public string? Region { get; init; }
    }
}
