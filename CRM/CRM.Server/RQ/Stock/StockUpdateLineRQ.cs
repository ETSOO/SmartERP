namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Stock update line request data
    /// 库存更新行请求数据
    /// </summary>
    public record StockUpdateLineRQ
    {
        /// <summary>
        /// Line id
        /// 行编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// New qty
        /// 新数量
        /// </summary>
        public decimal Qty { get; init; }
    }
}
