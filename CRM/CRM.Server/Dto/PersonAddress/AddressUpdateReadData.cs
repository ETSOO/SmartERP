using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.ApiModel.RQ.Maps;
using PlatformShared.Database.Models;

namespace CRM.Server.Dto.PersonAddress
{
    /// <summary>
    /// Address update read data
    /// 地址更新读取数据
    /// </summary>
    public record AddressUpdateReadData
    {
        /// <summary>
        /// ID
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Address kind
        /// 地址类型
        /// </summary>
        public AddressKind Kind { get; init; }

        /// <summary>
        /// Map provider
        /// 地图提供商
        /// </summary>
        public ApiProvider Provider { get; init; }

        /// <summary>
        /// Place id
        /// 地址编号
        /// </summary>
        public string? PlaceId { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Region
        /// 国家或地区
        /// </summary>
        public required string Region { get; init; }

        /// <summary>
        /// State
        /// 省或州
        /// </summary>
        public required string State { get; init; }

        /// <summary>
        /// City
        /// 城市
        /// </summary>
        public required string City { get; init; }

        /// <summary>
        /// District
        /// 区县
        /// </summary>
        public string? District { get; init; }

        /// <summary>
        /// Route
        /// 路径
        /// </summary>
        public string? Route { get; init; }

        /// <summary>
        /// Street
        /// 街道
        /// </summary>
        public string? Street { get; init; }

        /// <summary>
        /// Postal code
        /// 邮政编码
        /// </summary>
        public string? PostalCode { get; init; }

        /// <summary>
        /// Formatted address
        /// 格式化地址
        /// </summary>
        public required string FormattedAddress { get; init; }

        /// <summary>
        /// Location
        /// 位置
        /// </summary>
        public Location? Location { get; init; }
    }
}
