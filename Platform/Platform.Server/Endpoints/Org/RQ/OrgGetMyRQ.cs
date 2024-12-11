namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Get user's latest accessed organizations request data
    /// 获取用户最近访问的机构请求数据
    /// </summary>
    public record OrgGetMyRQ
    {
        /// <summary>
        /// Max items
        /// 最大项数
        /// </summary>
        public byte MaxItems { get; init; } = 10;
    }
}
