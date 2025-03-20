using PlatformShared.Database.Models;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Address item
    /// 地址项
    /// </summary>
    public record AddressItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public AddressKind Kind { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Formatted address
        /// 格式化地址
        /// </summary>
        public required string FormattedAddress { get; init; }
    }
}
