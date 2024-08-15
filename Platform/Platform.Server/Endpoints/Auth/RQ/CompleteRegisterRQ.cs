using com.etsoo.WebUtils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Platform.Server.Endpoints.Auth.RQ
{
    /// <summary>
    /// Complete register request data
    /// 完成注册请求数据
    /// </summary>
    public record CompleteRegisterRQ
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
        public required string Password { get; init; }

        /// <summary>
        /// Full name
        /// 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Country or region code, like CN = China
        /// 国家或地区编号，如 CN = 中国
        /// </summary>
        [RegionId]
        public required string Region { get; init; }
    }
}
