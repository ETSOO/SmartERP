using System.Net;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Core log
    /// 核心日志
    /// </summary>
    public class CoreLog
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Core user id
        /// 核心用户编号
        /// </summary>
        public int CoreUserId { get; set; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int? CoreOrganizationId { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// IP
        /// </summary>
        public IPAddress Ip { get; set; } = default!;

        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Device name
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; } = default!;

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public string Culture { get; set; } = default!;

        /// <summary>
        /// JSON data
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Core organization
        /// 核心机构
        /// </summary>
        public CoreOrganization CoreOrganization { get; set; } = default!;

        /// <summary>
        /// Core user
        /// 核心用户
        /// </summary>
        public CoreUser CoreUser { get; set; } = default!;

        /// <summary>
        /// Device
        /// 设备
        /// </summary>
        public CoreUserDevice Device { get; set; } = default!;
    }
}
