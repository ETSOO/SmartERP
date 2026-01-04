using PlatformShared.Database.Models;

namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Person info update item
    /// 人员信息更新项
    /// </summary>
    public record PersonInfoUpdateItem
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
        public bool? IsDefault { get; init; }
    }
}
