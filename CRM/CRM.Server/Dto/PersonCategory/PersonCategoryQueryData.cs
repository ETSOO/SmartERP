using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.PersonCategory
{
    /// <summary>
    /// Person category query data
    /// 人员分类查询数据
    /// </summary>
    public record PersonCategoryQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Names
        /// 名称列表
        /// </summary>
        public required IEnumerable<string> Names { get; init; }

        /// <summary>
        /// Identity Type
        /// 识别类型
        /// </summary>
        public required IdentityTypeFlags IdentityType { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
