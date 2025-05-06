namespace Platform.Server.Dto.Public
{
    /// <summary>
    /// Custom resource data
    /// 自定义资源数据
    /// </summary>
    public record CustomResourceData
    {
        /// <summary>
        /// Key
        /// 键名
        /// </summary>
        public required string Key { get; init; }

        /// <summary>
        /// Organization Id, null means global
        /// 所属机构，null 表示全局
        /// </summary>
        public int? OrgId { get; init; }

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
