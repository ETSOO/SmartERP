namespace CRM.Server.Dto.Stock
{
    /// <summary>
    /// Stock check item data
    /// 库存盘点项目数据
    /// </summary>
    public record StockCheckItemData
    {
        /// <summary>
        /// Order line id
        /// 订单项目编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long OrderId { get; init; }

        /// <summary>
        /// Qty.
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Qty delivered
        /// 已交付数量
        /// </summary>
        public decimal? QtyDelivered { get; init; }
    }
}
