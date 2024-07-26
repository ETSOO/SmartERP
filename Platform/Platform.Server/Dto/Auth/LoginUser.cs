using com.etsoo.CoreFramework.Business;

namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Login user data
    /// 登录用户数据
    /// </summary>
    public record LoginUser
    {
        public EntityStatus Status { get; init; }
        public DateTime? FrozenTime { get; init; }
        public short Step { get; init; }
    }
}
