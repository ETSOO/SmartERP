using com.etsoo.ApiModel.RQ.Maps;
using NpgsqlTypes;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person address
    /// 人员地址
    /// </summary>
    public class PersonAddress
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public AddressKind Kind { get; set; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Place id, from map provider
        /// 地址编号，来自地图提供商
        /// </summary>
        public string? PlaceId { get; set; }

        /// <summary>
        /// Region
        /// 地区
        /// </summary>
        public string Region { get; set; } = default!;

        /// <summary>
        /// State
        /// 州或者省
        /// </summary>
        public string State { get; set; } = default!;

        /// <summary>
        /// City
        /// 城市
        /// </summary>
        public string City { get; set; } = default!;

        /// <summary>
        /// District
        /// 区县
        /// </summary>
        public string? District { get; set; }

        /// <summary>
        /// Route
        /// 线路
        /// </summary>
        public string? Route { get; set; }

        /// <summary>
        /// Street and house number
        /// 街道和门牌号
        /// </summary>
        public string? Street { get; set; }

        /// <summary>
        /// Postal code
        /// 邮编
        /// </summary>
        public string? PostalCode { get; set; }

        /// <summary>
        /// Formatted address
        /// 格式化地址
        /// </summary>
        public string FormattedAddress { get; set; } = default!;

        /// <summary>
        /// Location coordinates
        /// 位置坐标
        /// </summary>
        public NpgsqlPoint? Location { get; set; }

        /// <summary>
        /// Map provider
        /// 地图提供商
        /// </summary>
        public ApiProvider Provider { get; set; }

        /// <summary>
        /// Creation time
        /// 创建时间
        /// </summary>
        public DateTime Creation { get; set; }

        /// <summary>
        /// Parent id
        /// 父地址编号
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Parent address
        /// 父地址
        /// </summary>
        public PersonAddress? Parent { get; set; }

        /// <summary>
        /// Person related
        /// 关联人员
        /// </summary>
        public Person Person { get; set; } = default!;

        /// <summary>
        /// Orders
        /// 订单
        /// </summary>
        public ICollection<OrderHeader> Orders { get; set; } = default!;

        /// <summary>
        /// Locations
        /// 位置
        /// </summary>
        public ICollection<PersonAddress> Locations { get; } = default!;
    }
}
