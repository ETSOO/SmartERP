using System.Text.Json.Serialization;

namespace CRM.Server.RQ.ProductCategory
{
    /// <summary>
    /// Product category list request data
    /// 产品分类列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(ProductCategoryQueryRQ))]
    public record ProductCategoryListRQ : QueryIntRQ
    {
        /// <summary>
        /// Parent category Id
        /// 父级分类编号
        /// </summary>
        public int? ParentId { get; init; }

        /// <summary>
        /// Parent and all descendant category ids
        /// 父级及所有下级子类编号
        /// </summary>
        public int? ParentIdAll { get; init; }

        /// <summary>
        /// Assigned ID
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }
    }
}
