using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto;
using PlatformShared.Database.Models;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Person
{
    /// <summary>
    /// Person list request data
    /// 人员列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(PersonQueryRQ))]
    public record PersonListRQ : QueryLongRQ, IQueryTag
    {
        /// <summary>
        /// Identity type
        /// 识别类型
        /// </summary>
        public IdentityTypeFlags? IdentityType { get; set; }

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
        /// Education
        /// 受教育程度
        /// </summary>
        public PersonEducation? Education { get; init; }

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
