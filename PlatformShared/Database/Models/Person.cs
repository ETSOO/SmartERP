using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person marital status
    /// 个人婚姻状况
    /// </summary>
    public enum PersonMaritalStatus : byte
    {
        /// <summary>
        /// Single
        /// 未婚
        /// </summary>
        Single = 1,

        /// <summary>
        /// Married
        /// 已婚
        /// </summary>
        Married = 2,

        /// <summary>
        /// Partnership
        /// 同居伴侣
        /// </summary>
        Partnership = 3,

        /// <summary>
        /// Separated
        /// 分居
        /// </summary>
        Separated = 4,

        /// <summary>
        /// Divorced
        /// 离异
        /// </summary>
        Divorced = 5,

        /// <summary>
        /// Widowed
        /// 丧偶
        /// </summary>
        Widowed = 6
    }

    /// <summary>
    /// Person education
    /// 个人学历
    /// </summary>
    public enum PersonEducation : byte
    {
        /// <summary>
        /// Primary school
        /// 小学
        /// </summary>
        PrimarySchool = 1,

        /// <summary>
        /// Year 2
        /// 小学2年级
        /// </summary>
        PrimarySchool2 = 2,

        /// <summary>
        /// Year 3
        /// 小学3年级
        /// </summary>
        PrimarySchool3 = 3,

        /// <summary>
        /// Year 4
        /// 小学4年级
        /// </summary>
        PrimarySchool4 = 4,

        /// <summary>
        /// Year 5
        /// 小学5年级
        /// </summary>
        PrimarySchool5 = 5,

        /// <summary>
        /// Year 6
        /// 小学6年级
        /// </summary>
        PrimarySchool6 = 6,

        /// <summary>
        /// Middle school
        /// 初中
        /// </summary>
        MiddleSchool = 20,

        /// <summary>
        /// Year 7
        /// 初中1年级
        /// </summary>
        MiddleSchool1 = 27,

        /// <summary>
        /// Year 8
        /// 初中2年级
        /// </summary>
        MiddleSchool2 = 28,

        /// <summary>
        /// Year 9
        /// 初中3年级
        /// </summary>
        MiddleSchool3 = 29,

        /// <summary>
        /// High school
        /// 高中
        /// </summary>
        HighSchool = 40,

        /// <summary>
        /// Year 10
        /// 高中1年级
        /// </summary>
        HighSchool1 = 51,

        /// <summary>
        /// Year 11
        /// 高中2年级
        /// </summary>
        HighSchool2 = 52,

        /// <summary>
        /// Year 12
        /// 高中3年级
        /// </summary>
        HighSchool3 = 53,

        /// <summary>
        /// Diploma Graduate
        /// 大专毕业
        /// </summary>
        University2 = 82,

        /// <summary>
        /// Graduated from Five Types of Higher Education (Self-study Exam, Open University, Night University, Party School, Correspondence Course)
        /// 五大学历毕业（自考、电大、夜大、党校、函授）
        /// </summary>
        University4 = 84,

        /// <summary>
        /// University Student
        /// 大学在读
        /// </summary>
        University6 = 86,

        /// <summary>
        /// Bachelor's Graduate (No Degree)
        /// 大学本科毕业（无学士学位）
        /// </summary>
        University8 = 88,

        /// <summary>
        /// Bachelor's Graduate (With Degree)
        /// 大学本科毕业且获得学士学位
        /// </summary>
        University10 = 90,

        /// <summary>
        /// Postgraduate (No Degree)
        /// 研究生毕业（无学位证书）
        /// </summary>
        University12 = 92,

        /// <summary>
        /// Master’s Graduate
        /// 硕士学位毕业
        /// </summary>
        University14 = 94
    }

    /// <summary>
    /// Person education degree
    /// 个人学位
    /// </summary>
    public enum PersonDegree : byte
    {
        /// <summary>
        /// Pre-bachelor
        /// 副学士
        /// </summary>
        PreBachelor = 1,

        /// <summary>
        /// Bachelor
        /// 学士
        /// </summary>
        Bachelor = 2,

        /// <summary>
        /// Master
        /// 硕士
        /// </summary>
        Master = 3,

        /// <summary>
        /// Doctor
        /// 博士
        /// </summary>
        Doctor = 4
    }

    /// <summary>
    /// Titles
    /// 称谓
    /// </summary>
    public enum PersonTitle : byte
    {
        /// <summary>
        /// Mr.
        /// 先生
        /// </summary>
        MR = 1,

        /// <summary>
        /// Ms.
        /// 女士
        /// </summary>
        MS = 2,

        /// <summary>
        /// Mrs.
        /// 夫人
        /// </summary>
        MRS = 3,

        /// <summary>
        /// Miss
        /// 小姐
        /// </summary>
        Miss = 4,

        /// <summary>
        /// Dr.
        /// 博士
        /// </summary>
        DR = 11,

        /// <summary>
        /// Prof.
        /// 教授
        /// </summary>
        Prof = 12
    }

    /// <summary>
    /// Individual and enterprise, presents an employee, customer, or supplier
    /// 个人和企业，表示员工、客户或供应商
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Female gender
        /// 女性
        /// </summary>
        public const string GenderFemale = "F";

        /// <summary>
        /// Male gender
        /// 男性
        /// </summary>
        public const string GenderMale = "M";

        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Unique identifier
        /// 唯一标识符
        /// </summary>
        public Guid Uid { get; set; }

        /// <summary>
        /// Organization (owner) Id
        /// 所属机构（所有者）编号
        /// </summary>
        public int OrgId { get; set; }

        /// <summary>
        /// Core organization Id related
        /// 关联的核心机构编号
        /// </summary>
        public int? CoreOrganizationId { get; set; }

        /// <summary>
        /// Core user Id related
        /// 关联的核心用户编号
        /// </summary>
        public int? CoreUserId { get; set; }

        /// <summary>
        /// User role, permission level
        /// 用户角色，权限等级
        /// </summary>
        public UserRole? UserRole { get; set; }

        /// <summary>
        /// Identity type, employee, customer, or supplier
        /// 标识类型，员工、客户或供应商
        /// </summary>
        public IdentityTypeFlags IdentityType { get; set; }

        /// <summary>
        /// Is legal person (enterprise)
        /// 是否为法人（企业）
        /// </summary>
        public bool IsLegalPerson { get; set; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Given name
        /// 名
        /// </summary>
        public string? GivenName { get; set; }

        /// <summary>
        /// Family name
        /// 姓
        /// </summary>
        public string? FamilyName { get; set; }

        /// <summary>
        /// Latin given name
        /// 拉丁名
        /// </summary>
        public string? LatinGivenName { get; set; }

        /// <summary>
        /// Latin family name
        /// 拉丁姓
        /// </summary>
        public string? LatinFamilyName { get; set; }

        /// <summary>
        /// Preferred name
        /// 首先名
        /// </summary>
        public string? PreferredName { get; set; }

        /// <summary>
        /// Titles
        /// 称谓
        /// </summary>
        public PersonTitle? Title { get; set; }

        /// <summary>
        /// Job title
        /// 工作头衔
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Avatar
        /// 头像
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// Birthday
        /// 生日
        /// </summary>
        public DateTimeOffset? Birthday { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; set; }

        /// <summary>
        /// Regions
        /// 地区
        /// </summary>
        public List<string>? Regions { get; set; }

        /// <summary>
        /// Currencies
        /// 币种
        /// </summary>
        public List<string>? Currencies { get; set; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public List<string>? Cultures { get; set; }

        /// <summary>
        /// Ethnicity
        /// 种族
        /// </summary>
        public string? Ethnicity { get; set; }

        /// <summary>
        /// Gender
        /// 性别
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Height in cm
        /// 高度（厘米）
        /// </summary>
        public short? Height { get; set; }

        /// <summary>
        /// Weight in kg
        /// 重量（千克）
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// Marital status
        /// 婚姻状况
        /// </summary>
        public PersonMaritalStatus? MaritalStatus { get; set; }

        /// <summary>
        /// Education
        /// 学历
        /// </summary>
        public PersonEducation? Education { get; set; }

        /// <summary>
        /// Education degree
        /// 学位
        /// </summary>
        public PersonDegree? Degree { get; set; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Creation time
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Expiry time
        /// 到期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; set; }

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset RefreshTime { get; set; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; set; }

        /// <summary>
        /// Query keyword
        /// 查询关键字
        /// </summary>
        public string? QueryKeyword { get; set; }

        /// <summary>
        /// Inviter Id (core_user_id.id)
        /// 邀请人编号
        /// </summary>
        public int? InviterId { get; set; }

        /// <summary>
        /// Report to (person.id)
        /// 汇报对象
        /// </summary>
        public long? ReportTo { get; set; }

        /// <summary>
        /// Political status
        /// 政治面貌
        /// </summary>
        public string? PoliticalStatus { get; set; }

        /// <summary>
        /// Category Ids
        /// 所属类目编号
        /// </summary>
        public List<int>? CategoryIds { get; set; }

        /// <summary>
        /// Keywords (id)
        /// 关键词（编号）
        /// </summary>
        public List<int>? Keywords { get; set; }

        /// <summary>
        /// Addresses
        /// 地址
        /// </summary>
        public List<int>? Addresses { get; set; }

        /// <summary>
        /// Registrant's User id
        /// 登记人的用户编号
        /// </summary>
        public required int UserId { get; set; }

        /// <summary>
        /// Permission groups
        /// 权限组
        /// </summary>
        public List<int>? PermissionGroups { get; set; }

        /// <summary>
        /// Permission item included
        /// 包括的权限项
        /// </summary>
        public List<short>? PermissionIncluded { get; set; }

        /// <summary>
        /// Permission item excluded
        /// 排除的权限项
        /// </summary>
        public List<short>? PermissionExcluded { get; set; }

        /// <summary>
        /// Core user related
        /// 关联的核心用户
        /// </summary>
        public CoreUser? CoreUser { get; set; }

        /// <summary>
        /// Inviter
        /// 邀请人
        /// </summary>
        public CoreUser? Inviter { get; set; }

        /// <summary>
        /// Bound core organization
        /// 绑定的核心机构
        /// </summary>
        public CoreOrganization? CoreOrganization { get; set; }

        /// <summary>
        /// Organization belonged
        /// 所属机构
        /// </summary>
        public CoreOrganization Organization { get; set; } = default!;

        /// <summary>
        /// Report to user
        /// 汇报对象用户
        /// </summary>
        public Person? ReportToUser { get; set; }

        /// <summary>
        /// CRM setting
        /// 客户关系管理设置
        /// </summary>
        public SettingCrm? SettingCrm { get; set; }

        /// <summary>
        /// Direct reports
        /// 直接下属
        /// </summary>
        public ICollection<Person> DirectReports { get; } = default!;

        /// <summary>
        /// Contacts
        /// 联系人
        /// </summary>
        public ICollection<PersonRelation> Contacts { get; } = default!;

        /// <summary>
        /// Contact owners
        /// 联系人所有者
        /// </summary>
        public ICollection<PersonRelation> ContactOwners { get; } = default!;

        /// <summary>
        /// Information
        /// 信息
        /// </summary>
        public ICollection<PersonInfo> Infos { get; } = default!;

        /// <summary>
        /// Orders
        /// 订单
        /// </summary>
        public ICollection<OrderHeader> Orders { get; set; } = default!;

        /// <summary>
        /// Profiles related
        /// 关联的档案
        /// </summary>
        public ICollection<PersonProfile> Profiles { get; } = default!;

        /// <summary>
        /// Created profiles
        /// 创建的
        /// </summary>
        public ICollection<PersonProfile> CreatedProfiles { get; } = default!;

        /// <summary>
        /// Assigned profiles
        /// 分配的档案
        /// </summary>
        public ICollection<PersonProfile> AssignedProfiles { get; } = default!;

        /// <summary>
        /// Permission items
        /// 权限项
        /// </summary>
        public ICollection<PermissionItem> PermissionItems { get; } = default!;

        /// <summary>
        /// Profile attachments
        /// 档案附件
        /// </summary>
        public ICollection<PersonProfileAttachment> ProfileAttachments { get; } = default!;

        /// <summary>
        /// Profile links
        /// 档案链接
        /// </summary>
        public ICollection<PersonProfileLink> ProfileLinks { get; } = default!;

        /// <summary>
        /// Purchases
        /// 采购
        /// </summary>
        public ICollection<OrderHeader> Purchases { get; set; } = default!;
    }
}
