namespace PlatformShared.Dto
{
    /// <summary>
    /// Stock kind
    /// 库存类型
    /// </summary>
    public enum StockKind : byte
    {
        /// <summary>
        /// Order fulfillment
        /// 订单发货
        /// </summary>
        Order = 1,

        /// <summary>
        /// Purchase receipt
        /// 采购入库
        /// </summary>
        PO = 10,

        /// <summary>
        /// Stock transfer
        /// 调货
        /// </summary>
        StockTransfer = 50,

        /// <summary>
        /// Stock taking
        /// 盘库
        /// </summary>
        StockTaking = 80,

        /// <summary>
        /// Quick assembly
        /// 快速组装
        /// </summary>
        Assembly = 100,

        /// <summary>
        /// Production
        /// 生产
        /// </summary>
        Production = 120,

        /// <summary>
        /// Loss reporting
        /// 报损
        /// </summary>
        Loss = 150
    }
}
