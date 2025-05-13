namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Get user's latest accessed organizations request data
    /// 获取用户最近访问的机构请求数据
    /// </summary>
    public record OrgGetMyRQ
    {
        /// <summary>
        /// Check if owns the application
        /// 是否拥有应用
        /// </summary>
        public int? AppId { get; init; }

        /// <summary>
        /// Max items
        /// 最大项数
        /// </summary>
        public byte MaxItems { get; init; } = 10;
    }
}
