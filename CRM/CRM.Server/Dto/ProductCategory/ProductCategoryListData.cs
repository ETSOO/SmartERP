using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.ProductCategory
{
    /// <summary>
    /// Product category list data
    /// 产品分类列表数据
    /// </summary>
    public record ProductCategoryListData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Assigned ID
        /// 分配编号
        /// </summary>
        public string? AssignedId { get; init; }
    }
}
