using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;

namespace Admin.Server.Dto.Query
{
    /// <summary>
    /// Read User Data
    /// 读取用户数据
    /// </summary>
    public record ReadUserDto
    {
        /// <summary>
        /// Identifier
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Display name
        /// 显示名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// ID
        /// 身份证号码
        /// </summary>
        public string? Pin { get; init; }

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
        /// Preferred name
        /// 首选名
        /// </summary>
        public string? PreferredName { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }

        /// <summary>
        /// Organization belongs to
        /// 加入的机构
        /// </summary>
        public required IEnumerable<IdNameItem> Orgs { get; init; }

        /// <summary>
        /// Devices
        /// 设备
        /// </summary>
        public required IEnumerable<IdNameItem> Devices { get; init; }
    }
}
