using System.Text.Json;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Document read data
    /// 文档读取数据
    /// </summary>
    public record DocumentReadData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Organizaton id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public required string Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Parameters
        /// 参数
        /// </summary>
        public JsonDocument? Parameters { get; init; }

        /// <summary>
        /// Template
        /// 模板
        /// </summary>
        public required string Template { get; init; }

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset RefreshTime { get; init; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public IEnumerable<string>? Cultures { get; init; }
    }
}
