using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Stock query product request data
    /// 库存查询产品请求数据
    /// </summary>
    public record StockQueryProductRQ : QueryIntRQ
    {
        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int? LocationId { get; init; }

        /// <summary>
        /// Scope
        /// 范围
        /// </summary>
        public ProductScope? Scope { get; init; }

        /// <summary>
        /// Usage
        /// 用途
        /// </summary>
        public ProductUsage? Usage { get; init; }

        /// <summary>
        /// Category
        /// 所属分类
        /// </summary>
        public int? CategoryId { get; init; }

        /// <summary>
        /// Category and all descendant category ids
        /// 所属分类及所有下级子类编号
        /// </summary>
        public int? CategoryIdAll { get; init; }

        /// <summary>
        /// Categories
        /// 所属多个分类
        /// </summary>
        public IEnumerable<int>? CategoryIds { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Assigned id start
        /// 分配的编号开始
        /// </summary>
        public string? AssignedIdStart { get; init; }

        /// <summary>
        /// Unit id
        /// 单位编号
        /// </summary>
        public int? UnitId { get; init; }
    }
}
