using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Person duplicate test data
    /// 人员重复测试数据
    /// </summary>
    public record PersonDuplicateTestData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 名称 / 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Identity type
        /// 识别类型
        /// </summary>
        public IdentityTypeFlags IdentityType { get; init; }
    }
}
