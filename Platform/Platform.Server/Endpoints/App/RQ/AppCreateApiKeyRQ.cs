using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.App.RQ
{
    /// <summary>
    /// Create API key request data
    /// 创建API密钥请求数据
    /// </summary>
    public record AppCreateApiKeyRQ : IModelValidator
    {
        /// <summary>
        /// App id
        /// 应用编号
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public required string DeviceId { get; init; }

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

            return null;
        }
    }
}
