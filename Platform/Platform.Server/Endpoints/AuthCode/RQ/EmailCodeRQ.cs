using com.etsoo.CoreFramework.Application;
using com.etsoo.Localization;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Database.Models;

namespace Platform.Server.Endpoints.AuthCode.RQ
{
    /// <summary>
    /// Email code request data
    /// 邮件验证码请求数据
    /// </summary>
    public record EmailCodeRQ : IModelValidator
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public required string DeviceId { get; init; }

        /// <summary>
        /// Action
        /// 操作
        /// </summary>
        public required AuthCodeAction Action { get; set; }

        /// <summary>
        /// User's email
        /// 用户邮箱
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// Country or region code, like CN = China
        /// 国家或地区编号，如 CN = 中国
        /// </summary>
        public string? Region { get; init; }

        /// <summary>
        /// Current's time zone
        /// 所在时区
        /// </summary>
        public string? TimeZone { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (DeviceId.Length is not (>= 32 and <= 512))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(DeviceId));
            }

            if (Email.Length is not (>= 64 and <= 512))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Email));
            }

            if (Region != null && Region.Length is not 2)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Region));
            }

            if (TimeZone != null && !LocalizationUtils.IsTimeZone(TimeZone))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(TimeZone));
            }

            return null;
        }
    }
}
