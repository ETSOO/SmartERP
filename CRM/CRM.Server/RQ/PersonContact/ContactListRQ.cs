using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto;
using PlatformShared.Database.Models;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.PersonContact
{
    /// <summary>
    /// Contact list request data
    /// 联系人列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(ContactQueryRQ))]
    public record ContactListRQ : QueryLongRQ, IQueryTag
    {
        /// <summary>
        /// Person ID
        /// 相关人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Relation type
        /// 关系类型
        /// </summary>
        public PersonRelationType? RelationType { get; init; }

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
        /// Categories
        /// 所属多个分类
        /// </summary>
        public IEnumerable<int>? CategoryIds { get; init; }

        /// <summary>
        /// City
        /// 所在城市
        /// </summary>
        public string? City { get; init; }

        public override IActionResult? Validate()
        {
            var result = base.Validate();

            if (result != null)
                return result;

            if (Tag != null && Tag.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Tag));
            }

            if (City != null && City.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(City));
            }

            return result;
        }
    }
}
