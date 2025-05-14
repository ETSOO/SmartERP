namespace CRM.Server.Dto.Group
{
    /// <summary>
    /// Permission group list data
    /// 权限组列表数据
    /// </summary>
    public record GroupListData
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
