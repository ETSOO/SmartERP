namespace Platform.Server.Endpoints.App.RQ
{
    /// <summary>
    /// Application buy request data
    /// 购买应用请求数据
    /// </summary>
    public record AppBuyRQ
    {
        /// <summary>
        /// Application ID
        /// 应用编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Organization ID
        /// 机构编号
        /// </summary>
        public int OrganizationId { get; init; }
    }
}
