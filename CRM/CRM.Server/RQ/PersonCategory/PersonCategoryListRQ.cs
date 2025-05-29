using com.etsoo.CoreFramework.Business;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.PersonCategory
{
    /// <summary>
    /// Person category list request data
    /// 人员分类列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(PersonCategoryQueryRQ))]
    public record PersonCategoryListRQ : QueryIntRQ
    {
        /// <summary>
        /// Identity type
        /// 识别类型
        /// </summary>
        public IdentityTypeFlags? IdentityType { get; init; }

        /// <summary>
        /// Parent category Id
        /// 父级分类编号
        /// </summary>
        public int? ParentId { get; init; }
    }
}
