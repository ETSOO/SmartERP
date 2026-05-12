namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Stock line
    /// 库存行
    /// </summary>
    public class StockLine
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Stock id
        /// 库存编号
        /// </summary>
        public long StockId { get; set; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// Qty.
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// Order / PO line id
        /// 订单 / 采购行编号
        /// </summary>
        public long? OrderLineId { get; set; }

        /// <summary>
        /// Location
        /// 位置
        /// </summary>
        public PersonAddress Location { get; set; } = default!;

        /// <summary>
        /// Order / PO line
        /// 订单 / 采购行
        /// </summary>
        public OrderLine? OrderLine { get; set; }

        /// <summary>
        /// Product
        /// 产品
        /// </summary>
        public Product Product { get; set; } = default!;

        /// <summary>
        /// Stock
        /// 库存
        /// </summary>
        public StockHeader Stock { get; set; } = default!;
    }
}
