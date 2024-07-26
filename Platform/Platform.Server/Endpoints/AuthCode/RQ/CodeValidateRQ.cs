using System.ComponentModel.DataAnnotations;

namespace Platform.Server.Endpoints.AuthCode.RQ
{
    /// <summary>
    /// Code validate request data
    /// 验证码验证请求数据
    /// </summary>
    public record CodeValidateRQ
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        [StringLength(512, MinimumLength = 32)]
        public required string DeviceId { get; init; }

        /// <summary>
        /// Guid
        /// 唯一编号
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// Code
        /// 验证码
        /// </summary>
        [StringLength(256, MinimumLength = 32)]
        public required string Code { get; set; }
    }
}
