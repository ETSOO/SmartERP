using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Applications;
using System;
using System.IO;
using System.Threading.Tasks;

namespace com.etsoo.SmartERP.Address
{
    /// <summary>
    /// Address service
    /// 地址服务
    /// </summary>
    public class AddressService : ModuleService<int>, IAddressService
    {
        /// <summary>
        /// Create address service
        /// 创建地址服务
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">Current user</param>
        /// <param name="moduleId">Module id</param>
        /// <returns>Address service</returns>
        public static AddressService Create(IMainApp app, ICurrentUser user, byte moduleId)
        {
            var service = new AddressService(moduleId);
            Setup(service, app, user);
            return service;
        }

        /// <summary>
        /// Create address service from other host service
        /// 从其他宿主服务创建地址服务
        /// </summary>
        /// <param name="host">Host service</param>
        /// <returns>Address service</returns>
        public static AddressService Create<T, S>(S source) where T : struct, IComparable where S : MainService<T>, IAddressServiceHost
        {
            var service = new AddressService(source.ModuleId);
            Setup(service, source);
            return service;
        }

        /// <summary>
        /// Service identity
        /// 服务标识
        /// </summary>
        public override string Identity
        {
            get
            {
                return "address";
            }
        }

        /// <summary>
        /// Private constructor to prevent initialization
        /// 私有的构造函数防止实例化
        /// </summary>
        /// <param name="moduleId">Module id</param>
        private AddressService(byte moduleId) : base(moduleId)
        {
        }

        /// <summary>
        /// Async get city list JSON data
        /// 异步获得城市列表JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="region">Region</param>
        /// <param name="organizationId">Organization id</param>
        /// <param name="format">Data format</param>
        public async Task CityListAsync(Stream stream, string region, int? organizationId, DataFormat format)
        {
            await ExecuteAsync(stream, GetCityListData(region, organizationId, format), null);
        }

        /// <summary>
        /// Async get country list JSON data
        /// 异步获得国家列表JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="organizationId">Organization id</param>
        /// <param name="format">Data format</param>
        public async Task CountryListAsync(Stream stream, int? organizationId, DataFormat format)
        {
            await ExecuteAsync(stream, GetCountryListData(organizationId, format), null);
        }

        /// <summary>
        /// Async get district list JSON data
        /// 异步获得区县列表JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="city">City</param>
        /// <param name="organizationId">Organization id</param>
        /// <param name="format">Data format</param>
        public async Task DistrictListAsync(Stream stream, string city, int? organizationId, DataFormat format)
        {
            await ExecuteAsync(stream, GetDistrictListData(city, organizationId, format), null);
        }

        // Get city list operation data
        // 获取城市列表操作数
        private OperationData GetCityListData(string region, int? organizationId, DataFormat format)
        {
            // Create operation data
            var data = CreateFormatOperationData("city_list", null, format);

            // Region is required
            if (string.IsNullOrEmpty(region))
            {
                data.TestResult.SetError(-1, "region", "data_error");
            }
            else
            {
                data.Parameters.Add("region", region);
                data.Parameters.Add("organization_id", organizationId);
            }

            // Return
            return data;
        }

        // Get country list operation data
        // 获取国家列表操作数
        private OperationData GetCountryListData(int? organizationId, DataFormat format)
        {
            // Create operation data
            var data = CreateFormatOperationData("country_list", null, format);

            // Add parameter
            data.Parameters.Add("organization_id", organizationId);

            // Return
            return data;
        }

        // Get district list operation data
        // 获取区县列表操作数
        private OperationData GetDistrictListData(string city, int? organizationId, DataFormat format)
        {
            // Create operation data
            var data = CreateFormatOperationData("district_list", null, format);

            // City is required
            if (string.IsNullOrEmpty(city))
            {
                data.TestResult.SetError(-1, "city", "data_error");
            }
            else
            {
                data.Parameters.Add("city", city);
                data.Parameters.Add("organization_id", organizationId);
            }

            // Return
            return data;
        }

        // Get region list operation data
        // 获取地区列表操作数
        private OperationData GetRegionListData(string country, int? organizationId, DataFormat format)
        {
            // Create operation data
            var data = CreateFormatOperationData("region_list", null, format);

            // Country is required
            if(string.IsNullOrEmpty(country))
            {
                data.TestResult.SetError(-1, "country", "data_error");
            }
            else
            {
                data.Parameters.Add("country", country);
                data.Parameters.Add("organization_id", organizationId);
            }

            // Return
            return data;
        }

        /// <summary>
        /// Async get region list JSON data
        /// 异步获得地区列表JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="country">Country</param>
        /// <param name="organizationId">Organization id</param>
        /// <param name="format">Data format</param>
        public async Task RegionListAsync(Stream stream, string country, int? organizationId, DataFormat format)
        {
            await ExecuteAsync(stream, GetRegionListData(country, organizationId, format), null);
        }
    }
}