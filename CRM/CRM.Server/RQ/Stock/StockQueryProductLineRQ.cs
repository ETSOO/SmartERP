using PlatformShared.Dto;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Stock query product line request data
    /// 库存查询产品行请求数据
    /// </summary>
    public record StockQueryProductLineRQ : QueryLongRQ
    {
        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

        /// <summary>
        /// Stock kind
        /// 库存类型
        /// </summary>
        public StockKind? StockKind { get; init; }

        /// <summary>
        /// Location id
        /// 仓库地址编号
        /// </summary>
        public int? LocationId { get; init; }

        /// <summary>
        /// Qty start
        /// 开始数量
        /// </summary>
        public decimal? QtyStart { get; init; }

        /// <summary>
        /// Qty end
        /// 结束数量
        /// </summary>
        public decimal? QtyEnd { get; init; }

        /// <summary>
        /// Total qty end
        /// Creation start
        /// 登记日期开始
        /// </summary>
        public DateTimeOffset? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 登记日期结束
        /// </summary>
        public DateTimeOffset? CreationEnd { get; init; }
    }
}
