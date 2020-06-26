using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Applications;
using System;

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

        
    }
}