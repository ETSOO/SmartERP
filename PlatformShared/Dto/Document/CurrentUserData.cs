using PlatformShared.Database.Models;

namespace PlatformShared.Dto.Document
{
    /// <summary>
    /// Current user data
    /// 当前用户数据
    /// </summary>
    public record CurrentUserData
    {
        /// <summary>
        /// Display name
        /// 显示名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Preferred name
        /// 首选姓名
        /// </summary>
        public string? PreferredName { get; init; }

        /// <summary>
        /// Given name
        /// 名
        /// </summary>
        public string? GivenName { get; init; }

        /// <summary>
        /// Family name
        /// 姓
        /// </summary>
        public string? FamilyName { get; init; }

        /// <summary>
        /// Latin given name
        /// 拉丁名（拼音）
        /// </summary>
        public string? LatinGivenName { get; init; }

        /// <summary>
        /// Latin family name
        /// 拉丁姓（拼音）
        /// </summary>
        public string? LatinFamilyName { get; init; }

        /// <summary>
        /// Avatar
        /// 头像
        /// </summary>
        public string? Avatar { get; init; }

        /// <summary>
        /// Signature
        /// 签名
        /// </summary>
        public string? Signature { get; init; }

        /// <summary>
        /// Infos
        /// 信息项目
        /// </summary>
        public required IEnumerable<PersonInfoViewItem> Infos { get; init; }
    }
}
