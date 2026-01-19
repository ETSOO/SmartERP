namespace CRM.Server.RQ.Asset
{
    /// <summary>
    /// Asset query request data
    /// 资产查询请求数据
    /// </summary>
    public record AssetQueryRQ : AssetListRQ
    {
        /// <summary>
        /// Operator's core user id
        /// 操作员的核心用户编号
        /// </summary>
        public int? UserId { get; init; }
    }
}