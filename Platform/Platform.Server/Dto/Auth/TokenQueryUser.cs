using com.etsoo.CoreFramework.Authentication;

namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Token query user data
    /// 令牌查询用户数据
    /// </summary>
    public record TokenQueryUser : LoginUser
    {
        public required string Name { get; init; }
        public string? Avatar { get; init; }
        public string? OrganizationName { get; init; }
        public int? Oid { get; init; }
        public UserRole? Role { get; init; }
    }
}
