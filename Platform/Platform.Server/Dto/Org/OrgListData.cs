namespace Platform.Server.Dto.Org
{
    /// <summary>
    /// Organization list data
    /// 机构列表数据
    /// </summary>
    public record OrgListData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Unique identifier
        /// 唯一标识
        /// </summary>
        public string? Pin { get; init; }
    }
}
