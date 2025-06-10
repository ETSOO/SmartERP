using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.String;
using com.etsoo.WebUtils.Attributes;
using CRM.Server.Dto.Person;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Person
{
    /// <summary>
    /// Person update request data
    /// 人员更新请求数据
    /// </summary>
    public record PersonUpdateRQ : UpdateModel<long>, IModelValidator
    {
        /// <summary>
        /// Identity type
        /// 识别类型
        /// </summary>
        public IdentityTypeFlags? IdentityType { get; init; }

        /// <summary>
        /// Is legal person (enterprise)
        /// 是否为法人（企业）
        /// </summary>
        public bool? IsLegalPerson { get; init; }

        /// <summary>
        /// Name
        /// 姓名
        /// </summary>
        public string? Name { get; init; }

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
        /// Latin given name
        /// 拉丁名
        /// </summary>
        public string? LatinGivenName { get; init; }

        /// <summary>
        /// Latin family name
        /// 拉丁姓
        /// </summary>
        public string? LatinFamilyName { get; init; }

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
        /// Titles
        /// 称谓
        /// </summary>
        public PersonTitle? Title { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Query keyword
        /// 查询关键字
        /// </summary>
        public string? QueryKeyword { get; init; }

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
        /// Addresses
        /// 地址
        /// </summary>
        public IEnumerable<int>? Addresses { get; init; }

        /// <summary>
        /// Report to (person.id)
        /// 汇报对象
        /// </summary>
        public long? ReportTo { get; init; }

        /// <summary>
        /// Regions
        /// 地区
        /// </summary>
        public IEnumerable<string>? Regions { get; init; }

        /// <summary>
        /// Currencies
        /// 币种
        /// </summary>
        public IEnumerable<string>? Currencies { get; init; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public IEnumerable<string>? Cultures { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; init; }

        /// <summary>
        /// Expiry time
        /// 到期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// Private data
        /// 隐私数据
        /// </summary>
        public PersonPrivateData? PrivateData { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Name != null && Name.Length is not (>= 1 and <= 128))
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

            if (LatinGivenName != null && LatinGivenName.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(LatinGivenName));
            }

            if (LatinFamilyName != null && LatinFamilyName.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(LatinFamilyName));
            }

            if (PreferredName != null && PreferredName.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(PreferredName));
            }

            if (JobTitle != null && JobTitle.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(JobTitle));
            }

            if (PrivateData?.Gender != null && PrivateData.Gender is not "F" or "M")
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(PrivateData.Gender));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 1280))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (AssignedId != null && AssignedId.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            if (PrivateData?.Ethnicity != null && PrivateData.Ethnicity.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(PrivateData.Ethnicity));
            }

            if (QueryKeyword != null && QueryKeyword.Length is not (>= 1 and <= 30))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(QueryKeyword));
            }

            if (Regions != null && Regions.Any(c => !new RegionIdAttribute().IsValid(c)))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Regions));
            }

            if (Currencies != null && Currencies.Any(c => !new CurrencyAttribute().IsValid(c)))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Currencies));
            }

            if (Cultures != null && Cultures.Any(c => !new LanguageCodeAttribute().IsValid(c)))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Cultures));
            }

            if (Tags != null && Tags.Any(t => t.Length is < 1 or > 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Tags));
            }

            if (Data != null && !Data.IsJson())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Data));
            }

            return null;
        }
    }
}
