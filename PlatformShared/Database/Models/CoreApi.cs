namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Core API service enumeration
    /// 核心接口服务枚举
    /// </summary>
    public enum CoreApiService : short
    {
        /// <summary>
        /// SMTP email
        /// 电子邮件
        /// </summary>
        SMTP = 1,

        /// <summary>
        /// Storage
        /// 存储
        /// </summary>
        Storage = 2
    }

    /// <summary>
    /// External APIs
    /// 外部接口
    /// </summary>
    public class CoreApi
    {
        /// <summary>
        /// ID
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Core Organization ID
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Service
        /// 服务
        /// </summary>
        public CoreApiService Service { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Endpoint URL
        /// 端点网址
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// App or user ID
        /// 程序或用户编号
        /// </summary>
        public string AppId { get; set; } = default!;

        /// <summary>
        /// App or user secret
        /// 程序或用户密钥
        /// </summary>
        public string AppSecret { get; set; } = default!;

        /// <summary>
        /// Options in JSON format
        /// JSON 格式的选项
        /// </summary>
        public string? Options { get; set; }

        /// <summary>
        /// Rate policy, various values for different services
        /// 频率政策，不同服务有不同的值
        /// </summary>
        public short? RatePolicy { get; set; }

        /// <summary>
        /// Access token
        /// 访问令牌
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset? RefreshTime { get; set; }

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Inheritance or not
        /// 是否继承
        /// </summary>
        public bool Inheritance { get; set; }

        /// <summary>
        /// Creation time
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Updated time
        /// 更新时间
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// Core Organization
        /// 核心机构
        /// </summary>
        public CoreOrganization CoreOrganization { get; set; } = default!;
    }
}
