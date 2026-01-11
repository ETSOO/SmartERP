using CRM.Server.Dto.Product;

namespace CRM.Server.RQ.Product
{
    /// <summary>
    /// Product unit update request data
    /// 产品单位更新请求数据
    /// </summary>
    public record ProductUnitUpdateRQ
    {
        /// <summary>
        /// Removed ids
        /// 移除的编号
        /// </summary>
        public IEnumerable<int>? RemovedIds { get; init; }

        /// <summary>
        /// Items
        /// 项目
        /// </summary>
        public required IEnumerable<ProductUnitItem> Items { get; init; }
    }
}
