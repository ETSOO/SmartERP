using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.PersonAddress
{
    /// <summary>
    /// Person address update request data
    /// 人员地址更新请求数据
    /// </summary>
    public record AddressUpdateRQ : UpdateModel<int>, IModelValidator
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long? PersonId { get; init; }

        /// <summary>
        /// Address kind
        /// 地址类型
        /// </summary>
        public AddressKind? Kind { get; init; }

        /// <summary>
        /// Map provider
        /// 地图提供商
        /// </summary>
        public ApiProvider? Provider { get; init; }

        /// <summary>
        /// Place id
        /// 地址编号
        /// </summary>
        public string? PlaceId { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Region
        /// 国家或地区
        /// </summary>
        public string? Region { get; init; }

        /// <summary>
        /// State
        /// 省或州
        /// </summary>
        public string? State { get; init; }

        /// <summary>
        /// City
        /// 城市
        /// </summary>
        public string? City { get; init; }

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
        public string? FormattedAddress { get; init; }

        /// <summary>
        /// Parent address id
        /// 父地址编号
        /// </summary>
        public int? ParentId { get; init; }

        /// <summary>
        /// Location
        /// 位置
        /// </summary>
        public Location? Location { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Name != null && Name.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (Region != null && !new RegionIdAttribute().IsValid(Region))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Region));
            }

            if (State != null && State.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(State));
            }

            if (City != null && City.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(City));
            }

            if (District != null && District.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(District));
            }

            if (Route != null && Route.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Route));
            }

            if (Street != null && Street.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Street));
            }

            if (PostalCode != null && PostalCode.Length is not (>= 1 and <= 10))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(PostalCode));
            }

            if (FormattedAddress != null && FormattedAddress.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(FormattedAddress));
            }

            return null;
        }
    }
}
