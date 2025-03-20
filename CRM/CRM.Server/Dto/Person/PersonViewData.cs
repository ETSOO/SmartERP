using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using PlatformShared.Database.Models;
using PlatformShared.Dto;

namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Person private data
    /// 人员私有数据
    /// </summary>
    public record PersonPrivateData
    {
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
        /// Ethnicity
        /// 种族
        /// </summary>
        public string? Ethnicity { get; init; }

        /// <summary>
        /// Height in cm
        /// 高度（厘米）
        /// </summary>
        public short? Height { get; init; }

        /// <summary>
        /// Weight in kg
        /// 重量（千克）
        /// </summary>
        public decimal? Weight { get; init; }

        /// <summary>
        /// Marital status
        /// 婚姻状况
        /// </summary>
        public PersonMaritalStatus? MaritalStatus { get; init; }

        /// <summary>
        /// Education
        /// 学历
        /// </summary>
        public PersonEducation? Education { get; init; }

        /// <summary>
        /// Education degree
        /// 学位
        /// </summary>
        public PersonDegree? Degree { get; init; }

        /// <summary>
        /// Political status
        /// 政治面貌
        /// </summary>
        public string? PoliticalStatus { get; init; }
    }

    /// <summary>
    /// Person view data
    /// 人员浏览数据
    /// </summary>
    public record PersonViewData : ContactItem
    {
        /// <summary>
        /// Unique identifier
        /// 唯一标识符
        /// </summary>
        public Guid Uid { get; init; }

        /// <summary>
        /// Is legal person (enterprise)
        /// 是否为法人（企业）
        /// </summary>
        public bool IsLegalPerson { get; init; }

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
        /// Avatar
        /// 头像
        /// </summary>
        public string? Avatar { get; init; }

        /// <summary>
        /// Job title
        /// 工作头衔
        /// </summary>
        public string? JobTitle { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<CategoryItem>? Categories { get; init; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        public IEnumerable<string>? Keywords { get; init; }

        /// <summary>
        /// Addresses
        /// 地址
        /// </summary>
        public IEnumerable<AddressItem>? Addresses { get; init; }

        /// <summary>
        /// Report to (person.id)
        /// 汇报对象
        /// </summary>
        public long? ReportTo { get; init; }

        /// <summary>
        /// Report to name
        /// 汇报对象姓名
        /// </summary>
        public string? ReportToName { get; init; }

        /// <summary>
        /// Creation time
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Query keyword
        /// 查询关键字
        /// </summary>
        public string? QueryKeyword { get; init; }

        /// <summary>
        /// Regions
        /// 地区
        /// </summary>
        public List<string>? Regions { get; init; }

        /// <summary>
        /// Currencies
        /// 币种
        /// </summary>
        public List<string>? Currencies { get; init; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public List<string>? Cultures { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; init; }

        /** User **/

        /// <summary>
        /// User role, permission level
        /// 用户角色，权限等级
        /// </summary>
        public UserRole? UserRole { get; init; }

        /// <summary>
        /// Inviter
        /// 邀请人编号
        /// </summary>
        public string? InviterName { get; init; }

        /// <summary>
        /// Expiry time
        /// 到期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; init; }

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset RefreshTime { get; init; }

        /// <summary>
        /// Private data
        /// 私有数据
        /// </summary>
        public PersonPrivateData? PrivateData { get; init; }
    }
}
