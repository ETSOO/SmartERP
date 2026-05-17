namespace CRM.Server.Dto.Stock
{
    /// <summary>
    /// Stock query line data
    /// 库存行查询数据
    /// </summary>
    public record StockQueryLineData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

        /// <summary>
        /// Product name
        /// 产品名称
        /// </summary>
        public required string ProductName { get; init; }

        /// <summary>
        /// Qty.
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Order / PO line id
        /// 订单 / 采购行编号
        /// </summary>
        public long? OrderLineId { get; init; } 
    }
}