namespace Platform.Server.Dto.Org
{
    /// <summary>
    /// Custom resource query data
    /// 自定义资源查询数据
    /// </summary>
    public record OrgQueryResourceData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Key
        /// 键名
        /// </summary>
        public required string Key { get; init; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public required string Culture { get; init; }

        /// <summary>
        /// Organization name
        /// 机构名称
        /// </summary>
        public string? OrgName { get; init; }

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
    }
}
