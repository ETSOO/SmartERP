using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Applications;
using com.etsoo.SmartERP.Login;
using System;

namespace com.etsoo.SmartERP.User
{
    /// <summary>
    /// User service
    /// 用户服务
    /// </summary>
    public sealed class UserSerivce : LoginService
    {
        /// <summary>
        /// Create user service
        /// 创建用户服务
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">Current user</param>
        /// <returns>User service</returns>
        public static UserSerivce Create(IMainApp app, ICurrentUser user)
        {
            var service = new UserSerivce();
            Setup(service, app, user);
            return service;
        }

        /// <summary>
        /// Create user service from other source service
        /// 从其他源服务创建用户服务
        /// </summary>
        /// <param name="source">Source service</param>
        /// <returns>User service</returns>
        public static UserSerivce Create<T>(MainService<T> source) where T : struct, IComparable
        {
            var service = new UserSerivce();
            Setup(service, source);
            return service;
        }

        /// <summary>
        /// Private constructor to prevent initialization
        /// 私有的构造函数防止实例化
        /// </summary>
        private UserSerivce()
        {

        }

        /// <summary>
        /// Service identity
        /// 服务标识
        /// </summary>
        public override string Identity
        {
            get
            {
                return "user";
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
                return (byte)AppModule.User;
            }
        }
    }
}