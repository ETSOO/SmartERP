using System.Net;

namespace PlatformShared.LogDatabase.Models
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
        /// Kind
        /// 类型
        /// </summary>
        public string Kind { get; set; } = default!;

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int? OrganizationId { get; set; }

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
        public int? DeviceId { get; set; }

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
    }
}
