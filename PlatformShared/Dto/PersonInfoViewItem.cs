using PlatformShared.Database.Models;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Person info view item
    /// 人员信息视图项目
    /// </summary>
    public record PersonInfoViewItem
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonInfoKind Kind { get; init; }

        /// <summary>
        /// Identifier
        /// 标识
        /// </summary>
        public required string Identifier { get; init; }

        /// <summary>
        /// Is default or not
        /// 是否默认
        /// </summary>
        public bool IsDefault { get; init; }

        /// <summary>
        /// Is verified or not
        /// 是否验证
        /// </summary>
        public bool IsVerified { get; init; }
    }
}
