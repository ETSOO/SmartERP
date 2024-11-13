using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.Auth.RQ
{
    /// <summary>
    /// SMS code request data
    /// 短信验证码请求数据
    /// </summary>
    public record SMSCodeRQ : IModelValidator
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
        public required short Action { get; init; }

        /// <summary>
        /// User's mobile
        /// 用户手机号码
        /// </summary>
        public required string Mobile { get; init; }

        /// <summary>
        /// Country or region code, like CN = China
        /// 国家或地区编号，如 CN = 中国
        /// </summary>
        public string? Region { get; init; }

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

            if (Mobile.Length is not (>= 64 and <= 512))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Mobile));
            }

            if (Region != null && Region.Length is not 2)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Region));
            }

            return null;
        }
    }
}
