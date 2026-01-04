using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.PersonCategory
{
    /// <summary>
    /// Person category duplicate test data
    /// 人员分类重复测试数据
    /// </summary>
    public record PersonCategoryDuplicateTestData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Names
        /// 名称数组
        /// </summary>
        public required IEnumerable<string> Names { get; init; }

        /// <summary>
        /// Identity type
        /// 识别类型
        /// </summary>
        public IdentityTypeFlags IdentityType { get; init; }
    }
}
