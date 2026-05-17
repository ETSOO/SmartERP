namespace CRM.Server.Dto.Stock
{
    /// <summary>
    /// Stock query product line data
    /// 库存查询产品行数据
    /// </summary>
    public record StockQueryProductLineData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Stock id
        /// 库存编号
        /// </summary>
        public long StockId { get; init; }

        /// <summary>
        /// Stock title
        /// 库存标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int LocationId { get; init; }

        /// <summary>
        /// Location name
        /// 位置名称
        /// </summary>
        public required string LocationName { get; init; }

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

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
