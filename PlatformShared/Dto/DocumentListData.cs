using System.Text.Json;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Document list data
    /// 文档列表数据
    /// </summary>
    public record DocumentListData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

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
    }
}
