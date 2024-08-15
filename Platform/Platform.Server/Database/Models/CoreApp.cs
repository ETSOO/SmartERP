namespace Platform.Server.Database.Models
{
    /// <summary>
    /// Identity type
    /// 标识类型
    /// </summary>
    [Flags]
    public enum IdentityType : byte
    {
        /// <summary>
        /// User
        /// 用户
        /// </summary>
        User = 1,

        /// <summary>
        /// Customer
        /// 客户
        /// </summary>
        Customer = 2,

        /// <summary>
        /// Supplier
        /// 供应商
        /// </summary>
        Supplier = 4,
    }

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
        public short Id { get; set; }

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
        /// Web URL
        /// 网址
        /// </summary>
        public required string WebUrl { get; set; }

        /// <summary>
        /// Help URL
        /// 帮助网址
        /// </summary>
        public string? HelpUrl { get; set; }

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
        /// Require local URL
        /// 是否需要本地网址
        /// </summary>
        public bool? RequireLocalUrl { get; set; }

        /// <summary>
        /// Core organization apps
        /// 核心机构应用
        /// </summary>
        public ICollection<CoreOrganizationApp> CoreOrganizationApps { get; set; } = [];
    }
}
