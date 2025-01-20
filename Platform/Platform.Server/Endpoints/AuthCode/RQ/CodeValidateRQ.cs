using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.AuthCode.RQ
{
    /// <summary>
    /// Code validate request data
    /// 验证码验证请求数据
    /// </summary>
    public record CodeValidateRQ : IModelValidator
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
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
        public required string Code { get; init; }

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

            if (Code.Length is not (>= 32 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Code));
            }

            return null;
        }
    }
}
