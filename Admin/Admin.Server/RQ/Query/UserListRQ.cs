namespace Admin.Server.RQ.Query
{
    /// <summary>
    /// User list request data
    /// 用户列表请求数据
    /// </summary>
    public record UserListRQ : QueryIntRQ
    {
        /// <summary>
        /// Organization id
        /// 所属机构
        /// </summary>
        public int? OrgId { get; init; }

        /// <summary>
        /// Exclude self
        /// 排除自己
        /// </summary>
        public bool? ExcludeSelf { get; init; }
    }
}