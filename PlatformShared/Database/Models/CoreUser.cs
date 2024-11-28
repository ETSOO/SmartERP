using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Core user registration step
    /// 核心用户注册步骤
    /// </summary>
    public static class CoreUserStep
    {
        /// <summary>
        /// Completed
        /// 已完成
        /// </summary>
        public const short Completed = 0;

        /// <summary>
        /// OAuth2
        /// 第三方登录
        /// </summary>
        public const short OAuth = 10;

        /// <summary>
        /// Email
        /// 邮箱
        /// </summary>
        public const short Email = 20;

        /// <summary>
        /// Mobile
        /// 手机号
        /// </summary>
        public const short Mobile = 30;

        /// <summary>
        /// Password
        /// 设置密码
        /// </summary>
        public const short Password = 40;
    }

    public class CoreUserLogin
    {
        /// <summary>
        /// Identifier
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Display name
        /// 显示名称
        /// </summary>
        public string Name { get; set; } = default!;

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
        /// 拉丁名（拼音）
        /// </summary>
        public string? LatinGivenName { get; set; }

        /// <summary>
        /// Latin family name
        /// 拉丁姓（拼音）
        /// </summary>
        public string? LatinFamilyName { get; set; }

        /// <summary>
        /// Avatar
        /// 头像
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// Latest visited organization id
        /// 最近访问的机构编号
        /// </summary>
        public List<int>? LatestOrganizationIds { get; set; }

        /// <summary>
        /// Latest visited application id
        /// 最近访问的程序编号
        /// </summary>
        public List<int>? LatestAppIds { get; set; }
    }

    /// <summary>
    /// Core user
    /// 核心用户
    /// </summary>
    public class CoreUser : CoreUserLogin
    {
        /// <summary>
        /// Password
        /// 密码
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Preferred name
        /// 首选姓名
        /// </summary>
        public string? PreferredName { get; set; }

        /// <summary>
        /// Frozen expiry time
        /// 冻结到期时间
        /// </summary>
        public DateTime? FrozenTime { get; set; }

        /// <summary>
        /// Registration step, 0 for completed
        /// 注册步骤，0为完成
        /// </summary>
        public short Step { get; set; }

        /// <summary>
        /// Region
        /// 国家或地区
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// PIN
        /// 个人身份证号
        /// </summary>
        public string? Pin { get; set; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; } = EntityStatus.Normal;

        /// <summary>
        /// Query keyword
        /// 查询关键字，中文下默认使用拼音首字母
        /// </summary>
        public string? QueryKeyword { get; set; }

        /// <summary>
        /// Core user authentication codes
        /// 核心用户认证验证码
        /// </summary>
        public ICollection<CoreAuthCode> CoreUserAuthCodes { get; set; } = [];

        /// <summary>
        /// Core organizations
        /// 核心机构
        /// </summary>
        public ICollection<CoreOrganization> CoreOrganizations { get; set; } = [];

        /// <summary>
        /// Core organization users
        /// 核心机构用户
        /// </summary>
        public ICollection<CoreOrganizationUser> CoreOrganizationUsers { get; set; } = [];

        /// <summary>
        /// Core user devices
        /// 核心用户设备
        /// </summary>
        public ICollection<CoreUserDevice> CoreUserDevices { get; set; } = [];

        /// <summary>
        /// Core user identifiers
        /// 核心用户登录编号
        /// </summary>
        public ICollection<CoreUserIdentifier> CoreUserIdentifiers { get; set; } = [];
    }
}