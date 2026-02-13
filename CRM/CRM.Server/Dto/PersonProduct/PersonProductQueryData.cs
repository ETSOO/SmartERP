using PlatformShared.Dto;

namespace CRM.Server.Dto.PersonProduct
{
    /// <summary>
    /// Person product query data
    /// 人员个性化产品查询数据
    /// </summary>
    public record PersonProductQueryData
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

        /// <summary>
        /// Product name
        /// 产品名称
        /// </summary>
        public required string ProductName { get; init; }

        /// <summary>
        /// Product assigned id
        /// 产品分配编号
        /// </summary>
        public string? ProductAssignedId { get; init; }

        /// <summary>
        /// Custom assigned ID
        /// 自定义分配编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public IEnumerable<ProductCustomData>? Cultures { get; init; }
    }
}
