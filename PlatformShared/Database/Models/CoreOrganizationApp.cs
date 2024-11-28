using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Core organization application
    /// 核心机构应用
    /// </summary>
    public class CoreOrganizationApp
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Core application id
        /// 核心应用编号
        /// </summary>
        public int CoreAppId { get; set; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// App key
        /// 程序键名
        /// </summary>
        public required string AppKey { get; set; }

        /// <summary>
        /// App secret
        /// 程序密钥
        /// </summary>
        public required string AppSecret { get; set; }

        /// <summary>
        /// Local name
        /// 本地名称
        /// </summary>
        public string? LocalName { get; set; }

        /// <summary>
        /// Local UI URL
        /// 本地用户界面网址
        /// </summary>
        public string? LocalUrl { get; set; }

        /// <summary>
        /// Local API URLs
        /// 本地接口网址
        /// </summary>
        public string[]? LocalApis { get; set; }

        /// <summary>
        /// Local help URL
        /// 本地帮助网址
        /// </summary>
        public string? LocalHelpUrl { get; set; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Core application
        /// 核心应用
        /// </summary>
        public CoreApp CoreApp { get; set; } = default!;

        /// <summary>
        /// Core organization
        /// 核心机构
        /// </summary>
        public CoreOrganization CoreOrganization { get; set; } = default!;
    }
}
