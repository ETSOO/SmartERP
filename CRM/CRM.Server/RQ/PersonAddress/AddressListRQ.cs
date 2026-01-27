using PlatformShared.Database.Models;

namespace CRM.Server.RQ.PersonAddress
{
    /// <summary>
    /// Address list request data
    /// 地址列表请求数据
    /// </summary>
    public record AddressListRQ : QueryIntRQ
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Address kind
        /// 地址类型
        /// </summary>
        public AddressKind? Kind { get; init; }

        /// <summary>
        /// Is location or not
        /// 是否为地址
        /// </summary>
        public bool? IsLocation { get; init; }

        /// <summary>
        /// Parent id
        /// 父级编号
        /// </summary>
        public int? ParentId { get; init; }

        /// <summary>
        /// Place id
        /// 地址编号
        /// </summary>
        public string? PlaceId { get; init; }

        /// <summary>
        /// Include owner's addresses or not
        /// 是否包含拥有者的地址
        /// </summary>
        public bool? IncludeOwner { get; init; }
    }
}
