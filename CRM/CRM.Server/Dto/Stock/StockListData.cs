using PlatformShared.Dto;

namespace CRM.Server.Dto.Stock
{
    /// <summary>
    /// Stock list data
    /// 库存列表数据
    /// </summary>
    public record StockListData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public StockKind Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }
    }
}
