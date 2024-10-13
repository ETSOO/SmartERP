namespace Platform.Server.Database.Models
{
    /// <summary>
    /// Token response type
    /// 令牌响应类型
    /// </summary>
    public enum TokenResponseType : byte
    {
        /// <summary>
        /// Code
        /// 代码
        /// </summary>
        Code = 1,

        /// <summary>
        /// Token
        /// 令牌
        /// </summary>
        Token = 2
    }

    /// <summary>
    /// Core user device token data
    /// 核心用户设备令牌数据
    /// </summary>
    public record DeviceTokenData
    {
        /// <summary>
        /// Region
        /// 地区
        /// </summary>
        public required string Region { get; init; }

        /// <summary>
        /// Scopes, in EF JSON format, IList works while IEnumerable does not
        /// 权限范围
        /// </summary>
        public required IList<string> Scopes { get; init; }

        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public required int OrganizationId { get; init; }

        /// <summary>
        /// User global unique identifier
        /// 用户全局唯一标识符
        /// </summary>
        public Guid? Uid { get; init; }

        /// <summary>
        /// Parent organization id
        /// 父级机构编号
        /// </summary>
        public required int? ParentOrganizationId { get; init; }

        /// <summary>
        /// Channel organization id
        /// 渠道机构编号
        /// </summary>
        public required int? ChannelOrganizationId { get; init; }

        /// <summary>
        /// Redirect URI
        /// 跳转网址
        /// </summary>
        public Uri? RedirectUri { get; init; }

        /// <summary>
        /// Access type
        /// 访问类型
        /// </summary>
        public string? AccessType { get; init; }
    }

    /// <summary>
    /// Core user device token
    /// 核心用户设备令牌
    /// </summary>
    public class CoreUserDeviceToken
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// App id
        /// 程序编号
        /// </summary>
        public int? AppId { get; set; }

        /// <summary>
        /// App key id
        /// 程序键名编号
        /// </summary>
        public int? AppKeyId { get; set; }

        /// <summary>
        /// Response type
        /// 响应类型
        /// </summary>
        public TokenResponseType ResponseType { get; set; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public required string Culture { get; set; }

        /// <summary>
        /// Token
        /// 令牌
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTime Expiry { get; set; }

        /// <summary>
        /// Data
        /// 数据
        /// </summary>
        public required DeviceTokenData Data { get; set; }

        /// <summary>
        /// Related app
        /// 相关程序
        /// </summary>
        public CoreApp? App { get; set; }

        /// <summary>
        /// Organization application key
        /// 机构应用键名
        /// </summary>
        public CoreOrganizationAppKey? AppKey { get; set; }

        /// <summary>
        /// Related device
        /// 相关设备
        /// </summary>
        public CoreUserDevice Device { get; set; } = default!;
    }
}
