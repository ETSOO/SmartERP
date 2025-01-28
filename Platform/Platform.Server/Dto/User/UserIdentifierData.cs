using PlatformShared.Database.Models;

namespace Platform.Server.Dto.User
{
    /// <summary>
    /// User identifier data
    /// 用户标识数据
    /// </summary>
    public record UserIdentifierData
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

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public required DateTimeOffset Creation { get; init; }
    }
}
