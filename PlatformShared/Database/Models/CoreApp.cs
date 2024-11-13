using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Core application
    /// 核心应用
    /// </summary>
    public class CoreApp
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Identity type
        /// 标识类型
        /// </summary>
        public IdentityType IdentityType { get; set; }

        /// <summary>
        /// Application secret
        /// 程序密钥
        /// </summary>
        public required string AppSecret { get; set; }

        /// <summary>
        /// Web URL
        /// 网址
        /// </summary>
        public required string WebUrl { get; set; }

        /// <summary>
        /// API URL
        /// 接口网址
        /// </summary>
        public required string ApiUrl { get; set; }

        /// <summary>
        /// Help URL
        /// 帮助网址
        /// </summary>
        public string? HelpUrl { get; set; }

        /// <summary>
        /// Require local URL
        /// 是否需要本地网址
        /// </summary>
        public bool? RequireLocalUrl { get; set; }

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; set; }

        /// <summary>
        /// Is public
        /// 是否公开
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Enabled
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Core organization apps
        /// 核心机构应用
        /// </summary>
        public ICollection<CoreOrganizationApp> CoreOrganizationApps { get; set; } = [];

        /// <summary>
        /// Core user device tokens
        /// 核心用户设备令牌
        /// </summary>
        public ICollection<CoreUserDeviceToken> CoreUserDeviceTokens { get; set; } = [];
    }
}
