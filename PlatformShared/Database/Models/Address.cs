using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.CoreFramework.Business;
using NpgsqlTypes;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Address kind
    /// 地址类型
    /// </summary>
    public enum AddressKind : byte
    {
        Office = 1,
        Home = 2,
        Other = 9
    }


    /// <summary>
    /// Address
    /// 地址
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

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
        /// Postcode
        /// 邮编
        /// </summary>
        public string? Postcode { get; set; }

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
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; }

        /// <summary>
        /// Core organization Id
        /// 核心机构（订单所属机构）编号
        /// </summary>
        public int CoreOrganizationId { get; set; }
    }
}
