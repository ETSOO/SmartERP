namespace CRM.Server.Dto.System
{
    /// <summary>
    /// Custom culture item
    /// 自定义文化项目
    /// </summary>
    public record CustomCultureItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Json data
        /// JSON 数据
        /// </summary>
        public string? JsonData { get; init; }
    }
}
