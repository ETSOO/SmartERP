using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Database.Models;

namespace Platform.Server.Endpoints.Auth.RQ
{
    /// <summary>
    /// Check user identifier existance request data
    /// 检查用户标识是否存在请求数据
    /// </summary>
    public record CheckUserIdentifierRQ : IModelValidator
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public required string DeviceId { get; init; }

        /// <summary>
        /// Type
        /// 类型
        /// </summary>
        public required CoreUserIdentifierType Type { get; init; }

        /// <summary>
        /// Openid
        /// 公开编号
        /// </summary>
        public required string Openid { get; init; }

        /// <summary>
        /// Region
        /// 所在地区
        /// </summary>
        public required string Region { get; init; }

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

            if (Openid.Length is not (>= 64 and <= 512))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Openid));
            }

            if (Region.Length is not 2)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Region));
            }

            return null;
        }
    }
}
