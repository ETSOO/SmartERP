namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Core organization application key
    /// 核心机构应用密钥
    /// </summary>
    public class CoreOrganizationAppKey
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Core organization application id
        /// 核心机构应用编号
        /// </summary>
        public int CoreOrganizationAppId { get; set; }

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
        /// Local API URL
        /// 本地接口网址
        /// </summary>
        public string? LocalApi { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Core organization application belongs to
        /// 属于的核心机构应用
        /// </summary>
        public CoreOrganizationApp CoreOrganizationApp { get; set; } = default!;

        /// <summary>
        /// Core user device tokens
        /// 核心用户设备令牌
        /// </summary>
        public ICollection<CoreUserDeviceToken> CoreUserDeviceTokens { get; set; } = [];
    }
}
