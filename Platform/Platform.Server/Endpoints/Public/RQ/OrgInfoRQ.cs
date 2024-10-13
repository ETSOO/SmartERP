using System.ComponentModel.DataAnnotations;

namespace Platform.Server.Endpoints.Public.RQ
{
    /// <summary>
    /// Organization query public information request
    /// 获取机构公开信息请求
    /// </summary>
    public record OrgInfoRQ
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        [StringLength(512, MinimumLength = 32)]
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
    }
}