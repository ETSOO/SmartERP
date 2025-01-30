using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.Auth.RQ
{
    /// <summary>
    /// Reset password request data
    /// 重置密码请求数据
    /// </summary>
    public record ResetPasswordRQ : IModelValidator
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public required string DeviceId { get; init; }

        /// <summary>
        /// User's email or mobile
        /// 用户邮箱或者手机号码
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Login password
        /// 登录密码
        /// </summary>
        public required string Password { get; init; }

        /// <summary>
        /// Country or region code, like CN = China
        /// 国家或地区编号，如 CN = 中国
        /// </summary>
        public required string Region { get; init; }

        /// <summary>
        /// Timezone name
        /// 时区名称
        /// </summary>
        public required string Timezone { get; init; }

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

            if (Id.Length is not (>= 32 and <= 512))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(DeviceId));
            }

            if (Password.Length is not (>= 64 and <= 512))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Password));
            }

            if (Region.Length is not 2)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Region));
            }

            if (Timezone.Length is not (>= 3 and <= 64))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Timezone));
            }

            return null;
        }
    }
}
