using PlatformShared.Database.Models;

namespace CRM.Server.Dto.PersonAddress
{
    /// <summary>
    /// Address query data
    /// 地址查询数据
    /// </summary>
    public record AddressQueryData
    {
        /// <summary>
        /// ID
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Address kind
        /// 地址类型
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

        /// <summary>
        /// Parent address name
        /// 父地址名称
        /// </summary>
        public string? ParentName { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}