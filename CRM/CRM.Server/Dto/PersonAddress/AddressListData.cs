using PlatformShared.Database.Models;

namespace CRM.Server.Dto.PersonAddress
{
    /// <summary>
    /// Address list data
    /// 地址列表信息
    /// </summary>
    public record AddressListData
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
        /// City
        /// 城市
        /// </summary>
        public required string City { get; init; }

        /// <summary>
        /// Parent address name
        /// 父地址名称
        /// </summary>
        public string? ParentName { get; init; }
    }
}
