using PlatformShared.Database.Models;

namespace Admin.Server.Dto.Query
{
    /// <summary>
    /// User Identifier Item
    /// 用户标识项
    /// </summary>
    public record UserIdentifierItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// Type
        /// 类型
        /// </summary>
        public required CoreUserIdentifierType Type { get; init; }

        /// <summary>
        /// Value
        /// 值
        /// </summary>
        public required string Value { get; init; }
    }
}
