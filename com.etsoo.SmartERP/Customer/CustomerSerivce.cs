using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Address;
using com.etsoo.SmartERP.Applications;
using com.etsoo.SmartERP.Login;
using System;

namespace com.etsoo.SmartERP.Customer
{
    /// <summary>
    /// Customer service
    /// 客户服务
    /// </summary>
    public sealed class CustomerSerivce : LoginService, IAddressServiceHost
    {
        /// <summary>
        /// Create customer service
        /// 创建客户服务
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">Current user</param>
        /// <returns>Customer service</returns>
        public static CustomerSerivce Create(IMainApp app, ICurrentUser user)
        {
            var service = new CustomerSerivce();
            Setup(service, app, user);
            return service;
        }

        /// <summary>
        /// Create customer service from other source service
        /// 从其他源服务创建客户服务
        /// </summary>
        /// <param name="source">Source service</param>
        /// <returns>Customer service</returns>
        public static CustomerSerivce Create<T>(MainService<T> source) where T : struct, IComparable
        {
            var service = new CustomerSerivce();
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
                return "customer";
            }
        }

        /// <summary>
        /// Module id
        /// 模块编号
        /// </summary>
        public override byte ModuleId
        {
            get
            {
                return (byte)AppModule.Customer;
            }
        }

        IAddressService address;
        /// <summary>
        /// Address service interface
        /// 地址服务接口
        /// </summary>
        public IAddressService Address
        {
            get
            {
                if (address == null)
                {
                    address = AddressService.Create<int, CustomerSerivce>(this);
                }

                return address;
            }
        }

        /// <summary>
        /// Private constructor to prevent initialization
        /// 私有的构造函数防止实例化
        /// </summary>
        private CustomerSerivce()
        {

        }
    }
}