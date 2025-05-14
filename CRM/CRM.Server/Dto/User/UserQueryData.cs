namespace CRM.Server.Dto.User
{
    /// <summary>
    /// User query data
    /// 用户查询数据
    /// </summary>
    public record UserQueryData
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
