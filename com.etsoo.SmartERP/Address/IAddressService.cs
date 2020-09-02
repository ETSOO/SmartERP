using com.etsoo.Core.Services;
using System.IO;
using System.Threading.Tasks;

namespace com.etsoo.SmartERP.Address
{
    /// <summary>
    /// Service that support address functions
    /// 支持地址功能的服务
    /// </summary>
    public interface IAddressService
    {
        /// <summary>
        /// Async get city list JSON data
        /// 异步获得城市列表JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="region">Region</param>
        /// <param name="organizationId">Organization id</param>
        /// <param name="format">Data format</param>
        /// <returns>Is content wrote</returns>
        Task<bool> CityListAsync(Stream stream, string region, int? organizationId, DataFormat format);

        /// <summary>
        /// Async get country list JSON data
        /// 异步获得国家列表JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="organizationId">Organization id</param>
        /// <param name="format">Data format</param>
        /// <returns>Is content wrote</returns>
        Task<bool> CountryListAsync(Stream stream, int? organizationId, DataFormat format);

        /// <summary>
        /// Async get district list JSON data
        /// 异步获得区县列表JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="city">City</param>
        /// <param name="organizationId">Organization id</param>
        /// <param name="format">Data format</param>
        /// <returns>Is content wrote</returns>
        Task<bool> DistrictListAsync(Stream stream, string city, int? organizationId, DataFormat format);

        /// <summary>
        /// Async get region list JSON data
        /// 异步获得地区列表JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="country">Country</param>
        /// <param name="organizationId">Organization id</param>
        /// <param name="format">Data format</param>
        /// <returns>Is content wrote</returns>
        Task<bool> RegionListAsync(Stream stream, string country, int? organizationId, DataFormat format);
    }
}