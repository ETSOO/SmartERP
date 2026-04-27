using CRM.Server.Dto;
using PlatformShared.Database.Models;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Product
{
    /// <summary>
    /// Product list request data
    /// 产品列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(ProductQueryRQ))]
    public record ProductListRQ : QueryIntRQ, IQueryTag
    {
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
        /// Tag
        /// 标签
        /// </summary>
        public string? Tag { get; init; }

        /// <summary>
        /// Tag ID
        /// 标签编号
        /// </summary>
        public int? TagId { get; set; }

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
    }
}
