namespace CRM.Server.Dto.Stock
{
    /// <summary>
    /// Query order line item data
    /// 查询订单行项目数据
    /// </summary>
    public record StockQueryOrderLineItemData
    {
        /// <summary>
        /// Order line id
        /// 订单行编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long OrderId { get; init; }

        /// <summary>
        /// Order qty
        /// 订单数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Pending qty
        /// 待交付数量
        /// </summary>
        public decimal PendingQty { get; init; }
    }

    /// <summary>
    /// Query order line stock data
    /// 查询订单行库存数据
    /// </summary>
    public record StockQueryOrderLineData
    {
        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Purchase minimum unit
        /// 购买最小单位
        /// </summary>
        public decimal? StepQty { get; init; }

        /// <summary>
        /// Unit name
        /// 单位
        /// </summary>
        public required string UnitName { get; init; }

        /// <summary>
        /// Stock qty
        /// 库存数量
        /// </summary>
        public decimal StockQty { get; init; }

        /// <summary>
        /// Order qty
        /// 订单数量
        /// </summary>
        public decimal OrderQty { get; init; }

        /// <summary>
        /// Pending qty
        /// 待交付数量
        /// </summary>
        public decimal PendingQty { get; init; }

        /// <summary>
        /// Order line items
        /// 订单行项目
        /// </summary>
        public required IEnumerable<StockQueryOrderLineItemData> Lines { get; init; }
    }
}
