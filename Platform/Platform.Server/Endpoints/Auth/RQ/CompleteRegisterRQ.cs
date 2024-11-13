using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.Auth.RQ
{
    /// <summary>
    /// Complete register request data
    /// 完成注册请求数据
    /// </summary>
    public record CompleteRegisterRQ : IModelValidator
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public required string DeviceId { get; init; }

        /// <summary>
        /// Login password
        /// 登录密码
        /// </summary>
        public required string Password { get; init; }

        /// <summary>
        /// Full name
        /// 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Family name
        /// 姓氏
        /// </summary>
        public string? FamilyName { get; init; }

        /// <summary>
        /// Given name
        /// 名
        /// </summary>
        public string? GivenName { get; init; }

        /// <summary>
        /// Country or region code, like CN = China
        /// 国家或地区编号，如 CN = 中国
        /// </summary>
        public required string Region { get; init; }

        /// <summary>
        /// Authentication request
        /// 授权请求
        /// </summary>
        public AuthRequest? Auth { get; init; }

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

            if (Password.Length is not (>= 64 and <= 512))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Password));
            }

            if (Name.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (FamilyName?.Length > 50)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(FamilyName));
            }

            if (GivenName?.Length > 50)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(GivenName));
            }

            if (Region.Length is not 2)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Region));
            }

            return null;
        }
    }
}
