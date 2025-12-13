using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Person
{
    /// <summary>
    /// Contact create request data
    /// 创建联系人请求数据
    /// </summary>
    public record ContactCreateRQ : IModelValidator
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public int PersonId { get; init; }

        /// <summary>
        /// Titles
        /// 称谓
        /// </summary>
        public PersonTitle? Title { get; init; }

        /// <summary>
        /// Name
        /// 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Given name
        /// 名
        /// </summary>
        public string? GivenName { get; init; }

        /// <summary>
        /// Family name
        /// 姓
        /// </summary>
        public string? FamilyName { get; init; }

        /// <summary>
        /// Preferred name
        /// 首先名
        /// </summary>
        public string? PreferredName { get; set; }

        /// <summary>
        /// Job title
        /// 职务
        /// </summary>
        public string? JobTitle { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Gender
        /// 性别
        /// </summary>
        public string? Gender { get; init; }

        /// <summary>
        /// Birthday
        /// 生日
        /// </summary>
        public DateTimeOffset? Birthday { get; init; }

        /// <summary>
        /// Phone
        /// 电话
        /// </summary>
        public string? Phone { get; init; }

        /// <summary>
        /// Mobile
        /// 手机
        /// </summary>
        public string? Mobile { get; init; }

        /// <summary>
        /// Email
        /// 电子邮箱
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// Relation type
        /// 关系类型
        /// </summary>
        public PersonRelationType RelationType { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<int>? Categories { get; init; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        public IEnumerable<string>? Tags { get; init; }

        /// <summary>
        /// Regions
        /// 地区
        /// </summary>
        public IEnumerable<string>? Regions { get; init; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public IEnumerable<string>? Cultures { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Name.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (GivenName != null && GivenName.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(GivenName));
            }

            if (FamilyName != null && FamilyName.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(FamilyName));
            }

            if (PreferredName != null && PreferredName.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(PreferredName));
            }

            if (JobTitle != null && JobTitle.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(JobTitle));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 1280))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (Phone != null)
            {
                if (Phone.Length is < 1 or > 20)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(Phone));
                }

                var phoneResult = RQExtentions.ValidatePersonInfo(PersonInfoKind.Phone, Phone);
                if (phoneResult != null)
                    return phoneResult;
            }

            if (Mobile != null)
            {
                if (Mobile.Length is < 1 or > 20)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(Mobile));
                }

                var mobileResult = RQExtentions.ValidatePersonInfo(PersonInfoKind.Mobile, Mobile);
                if (mobileResult != null)
                    return mobileResult;
            }

            if (Email != null)
            {
                if (Email.Length is < 1 or > 256)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(Mobile));
                }

                var emailResult = RQExtentions.ValidatePersonInfo(PersonInfoKind.Email, Email);
                if (emailResult != null)
                    return emailResult;
            }

            if (Regions != null && Regions.Any(c => !new RegionIdAttribute().IsValid(c)))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Regions));
            }

            if (Cultures != null && Cultures.Any(c => !new LanguageCodeAttribute().IsValid(c)))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Cultures));
            }

            if (Tags != null && Tags.Any(t => t.Length is < 1 or > 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Tags));
            }

            return null;
        }
    }
}
