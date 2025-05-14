namespace CRM.Server.Dto.Group
{
    /// <summary>
    /// Permission group query data
    /// 权限组查询数据
    /// </summary>
    public record GroupQueryData
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
    }
}
