using System.Text.Json;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Document template
    /// 文档模板
    /// </summary>
    public class CoreDocument
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int? CoreOrganizationId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public string Kind { get; set; } = default!;

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Parameters
        /// 参数
        /// </summary>
        public JsonDocument? Parameters { get; set; }

        /// <summary>
        /// Template content
        /// 模板内容
        /// </summary>
        public string Template { get; set; } = default!;

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset RefreshTime { get; set; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public List<string>? Cultures { get; set; }

        /// <summary>
        /// Organization
        /// 机构
        /// </summary>
        public CoreOrganization? CoreOrganization { get; set; }
    }
}
