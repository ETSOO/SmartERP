using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.ProductCategory
{
    /// <summary>
    /// Product category duplicate test data
    /// 产品分类重复测试数据
    /// </summary>
    public record ProductCategoryDuplicateTestData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Names
        /// 名称数组
        /// </summary>
        public required IEnumerable<string> Names { get; init; }
    }
}
