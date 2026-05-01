namespace CRM.Server.RQ.POLine
{
    /// <summary>
    /// Purchase line query request data
    /// 采购行查询请求数据
    /// </summary>
    public record POLineQueryRQ : POLineListRQ
    {
        /// <summary>
        /// Qty start
        /// 开始数据
        /// </summary>
        public decimal? QtyStart { get; init; }
    }
}
