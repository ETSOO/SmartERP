namespace Platform.Server.Dto.Org
{
    /// <summary>
    /// Custom resource item
    /// 自定义资源项
    /// </summary>
    public record OrgResourceItem
    {
        /// <summary>
        /// Culture
        /// 语言文化
        /// </summary>
        public required string Culture { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string? Title { get; init; }

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

        /// <summary>
        /// Updated flag, logical operation value, 1 = Title, 2 = Description, 4 = JsonData
        /// 更新标志，逻辑运算值，1 = Title, 2 = Description, 4 = JsonData
        /// </summary>
        public byte UpdatedFlag { get; init; }
    }
}
