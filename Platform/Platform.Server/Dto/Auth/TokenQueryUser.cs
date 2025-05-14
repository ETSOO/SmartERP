using com.etsoo.CoreFramework.Authentication;

namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Token query user data
    /// 令牌查询用户数据
    /// </summary>
    public record TokenQueryUser : LoginUser
    {
        /// <summary>
        /// Name
        /// 姓名
        /// </summary>
        public required string Name { get; init; }

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
        public string? Avatar { get; init; }

        /// <summary>
        /// Organization name
        /// 机构名称
        /// </summary>
        public string? OrganizationName { get; init; }

        /// <summary>
        /// Organization user id
        /// 机构用户编号
        /// </summary>
        public long? Oid { get; init; }

        /// <summary>
        /// Person id (organization)
        /// 人员编号（机构）
        /// </summary>
        public required long? Pid { get; init; }

        /// <summary>
        /// User role
        /// 用户角色
        /// </summary>
        public UserRole? Role { get; init; }

        /// <summary>
        /// Latest accessed application id
        /// 最近访问的应用编号
        /// </summary>
        public int? LatestAppId { get; init; }
    }
}
