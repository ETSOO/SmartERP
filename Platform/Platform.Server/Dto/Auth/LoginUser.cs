using com.etsoo.CoreFramework.Business;

namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Login user data
    /// 登录用户数据
    /// </summary>
    public record LoginUser
    {
        public required int Id { get; init; }
        public EntityStatus Status { get; init; }
        public DateTimeOffset? FrozenTime { get; init; }
        public short Step { get; init; }
        public int? IdentifierId { get; init; }
        public EntityStatus? OrgStatus { get; init; }
        public DateTimeOffset? OrgExpiry { get; init; }
    }

    /// <summary>
    /// Login user with password
    /// 带密码的登录用户
    /// </summary>
    public record LoginUserWithPassword : LoginUser
    {
        public string? Password { get; init; }
    }
}
