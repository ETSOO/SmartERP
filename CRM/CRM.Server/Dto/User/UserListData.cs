namespace CRM.Server.Dto.User
{
    /// <summary>
    /// User list data
    /// 用户列表数据
    /// </summary>
    public record UserListData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }
    }
}
