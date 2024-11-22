using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.Public.RQ
{
    /// <summary>
    /// Organization query public information request
    /// 获取机构公开信息请求
    /// </summary>
    public record OrgInfoRQ : IModelValidator
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public required string DeviceId { get; init; }

        /// <summary>
        /// Application ID
        /// 程序编号
        /// </summary>
        public int? AppId { get; init; }

        /// <summary>
        /// Application key
        /// 程序键名
        /// </summary>
        public string? AppKey { get; init; }

        /// <summary>
        /// Organization unique identifier, manually activated
        /// 机构全局唯一标识，手动激活
        /// </summary>
        public Guid? OrgUid { get; init; }

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

            if (AppKey != null && AppKey.Length is not <= 256)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AppKey));
            }

            return null;
        }
    }
}