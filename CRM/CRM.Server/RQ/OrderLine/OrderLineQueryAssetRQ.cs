namespace CRM.Server.RQ.OrderLine
{
    /// <summary>
    /// Order line query asset request data
    /// 查询订单行资产请求数据
    /// </summary>
    public record OrderLineQueryAssetRQ : OrderLineListRQ
    {
        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// Creation start
        /// 创建开始
        /// </summary>
        public DateTimeOffset? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 创建结束
        /// </summary>
        public DateTimeOffset? CreationEnd { get; init; }
    }
}
