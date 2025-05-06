namespace Platform.Server.Dto.Org
{
    /// <summary>
    /// Custom resource read for update data
    /// 更新自定义资源读取数据
    /// </summary>
    public record OrgUpdateResourceReadData
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
        /// Organization id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; init; }

        /// <summary>
        /// Items
        /// 项目
        /// </summary>
        public required IEnumerable<OrgResourceItem> Items { get; init; }
    }
}
