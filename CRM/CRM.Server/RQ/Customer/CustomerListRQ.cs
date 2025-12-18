using CRM.Server.Dto.Person;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Customer
{
    /// <summary>
    /// Customer list request data
    /// 客户列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(CustomerQueryRQ))]
    public record CustomerListRQ : QueryLongRQ, IPersonTag
    {
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
        /// City
        /// 所在城市
        /// </summary>
        public string? City { get; init; }
    }
}
