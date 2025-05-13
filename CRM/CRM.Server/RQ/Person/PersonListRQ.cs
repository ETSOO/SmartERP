using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using PlatformShared.Database.Models;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Person
{
    /// <summary>
    /// Person list request data
    /// 人员列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(PersonQueryRQ))]
    public record PersonListRQ : QueryLongRQ
    {
        /// <summary>
        /// Identity type
        /// 识别类型
        /// </summary>
        public IdentityTypeFlags? IdentityType { get; set; }

        /// <summary>
        /// Job title
        /// 职位
        /// </summary>
        public string? JobTitle { get; init; }

        /// <summary>
        /// Education
        /// 受教育程度
        /// </summary>
        public PersonEducation? Education { get; init; }

        public override IActionResult? Validate()
        {
            var result = base.Validate();

            if (result != null)
                return result;

            if (JobTitle != null && JobTitle.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(JobTitle));
            }

            return result;
        }
    }
}
