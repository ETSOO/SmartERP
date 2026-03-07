using com.etsoo.CoreFramework.Business;
using PlatformShared.Database.Models;
using System.Text.Json;

namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Person update read data
    /// 人员更新读取数据
    /// </summary>
    public record PersonUpdateReadData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Identity type
        /// 识别类型
        /// </summary>
        public IdentityTypeFlags IdentityType { get; init; }

        /// <summary>
        /// Is legal person (enterprise)
        /// 是否为法人（企业）
        /// </summary>
        public bool IsLegalPerson { get; init; }

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
        public JsonDocument? Data { get; init; }

        /// <summary>
        /// Expiry time
        /// 到期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Private data
        /// 私有数据
        /// </summary>
        public PersonPrivateData? PrivateData { get; init; }
    }
}
