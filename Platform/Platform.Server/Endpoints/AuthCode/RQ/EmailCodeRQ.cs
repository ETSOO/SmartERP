using com.etsoo.WebUtils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Platform.Server.Endpoints.AuthCode.RQ
{
    /// <summary>
    /// Email code request data
    /// 邮件验证码请求数据
    /// </summary>
    public record EmailCodeRQ
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
        public required short Action { get; set; }

        /// <summary>
        /// User's email
        /// 用户邮箱
        /// </summary>
        [StringLength(512, MinimumLength = 64)]
        public required string Email { get; init; }

        /// <summary>
        /// Country or region code, like CN = China
        /// 国家或地区编号，如 CN = 中国
        /// </summary>
        [RegionId]
        public string? Region { get; init; }

        /// <summary>
        /// Current's time zone
        /// 所在时区
        /// </summary>
        public string? TimeZone { get; init; }
    }

}
