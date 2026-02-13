using PlatformShared.Dto;

namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Product read custom data
    /// 产品读取自定义数据
    /// </summary>
    public record ProductReadCustomData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public required IEnumerable<ProductCustomData> Cultures { get; init; }

        /// <summary>
        /// Prices
        /// 价格
        /// </summary>
        public required IEnumerable<ProductSimplePriceItem> Prices {  get; init; }
    }
}
