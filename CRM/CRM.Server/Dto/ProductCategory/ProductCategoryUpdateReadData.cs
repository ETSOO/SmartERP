using com.etsoo.CoreFramework.Business;
using System.Text.Json;

namespace CRM.Server.Dto.ProductCategory
{
    /// <summary>
    /// Product category update read data
    /// 产品分类更新读取数据
    /// </summary>
    public record ProductCategoryUpdateReadData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Parent Id
        /// 父级编号
        /// </summary>
        public int? ParentId { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                return Names.Last();
            }
        }

        /// <summary>
        /// Names
        /// 名称列表
        /// </summary>
        public required IEnumerable<string> Names { get; init; }

        /// <summary>
        /// Assigned ID
        /// 分配编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public JsonDocument? Data { get; init; }

        /// <summary>
        /// Attributes definition
        /// 属性定义
        /// </summary>
        public JsonDocument? Attributes { get; init; }
    }
}
