using PlatformShared.Dto;

namespace CRM.Server.Dto.Stock
{
    /// <summary>
    /// Stock line view data
    /// 库存行浏览数据
    /// </summary>
    public record StockLineViewData
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
        /// Stock kind
        /// 库存类型
        /// </summary>
        public StockKind StockKind { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

        /// <summary>
        /// Purchase minimum unit
        /// 购买最小单位
        /// </summary>
        public decimal? StepQty { get; init; }

        /// <summary>
        /// Order / PO line id
        /// 订单 / 采购行编号
        /// </summary>
        public long? OrderLineId { get; init; }

        /// <summary>
        /// Qty.
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Order qty
        /// 订单数量
        /// </summary>
        public decimal? OrderQty { get; init; }

        /// <summary>
        /// Pending qty
        /// 待交付数量
        /// </summary>
        public decimal? PendingQty { get; init; }

        /// <summary>
        /// Stock qty
        /// 库存数量
        /// </summary>
        public decimal StockQty { get; init; }
    }
}
