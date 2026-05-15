using CRM.Server.Dto.Stock;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Check stock request data
    /// 检查库存请求数据
    /// </summary>
    public record CheckStockRQ
    {
        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int LocationId { get; init; }

        /// <summary>
        /// Items
        /// 类型
        /// </summary>
        public required IEnumerable<StockItem> Items { get; init; }
    }
}
