namespace CRM.Server.Dto.Stock
{
    /// <summary>
    /// Stock item
    /// 库存项目
    /// </summary>
    public record StockItem
    {
        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }
    }

    /// <summary>
    /// Order stock item
    /// 订单库存项目
    /// </summary>
    public record StockOrderItem : StockItem
    {
        /// <summary>
        /// Order line id
        /// 订单行编号
        /// </summary>
        public long OrderLineId { get; init; }
    }
}
