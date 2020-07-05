using com.etsoo.Core.Application;
using com.etsoo.Core.Services;
using System;

namespace com.etsoo.SmartERP.Applications
{
    /// <summary>
    /// Main abstract service
    /// 主抽象服务
    /// </summary>
    /// <typeparam name="T">Id type generic</typeparam>
    public abstract class MainService<T> : Service<T>, IMainService<T> where T : struct, IComparable
    {
        /// <summary>
        /// Create customer service
        /// 创建客户服务
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">Current user</param>
        /// <returns>User service</returns>
        public static void Setup(MainService<T> service, IMainApp app, ICurrentUser user)
        {
            service.Application = app;
            service.User = user;
        }

        /// <summary>
        /// Create customer service from other source service
        /// 从其他源服务创建客户服务
        /// </summary>
        /// <param name="source">Source service</param>
        /// <returns>User service</returns>
        public static void Setup<T1>(MainService<T> service, MainService<T1> source) where T1 : struct, IComparable
        {
            service.Cached = true;
            Setup(service, source.Application, source.User);
        }

        IMainApp application;
        /// <summary>
        /// Appliction
        /// 程序对象
        /// </summary>
        public new IMainApp Application
        {
            get
            {
                return application;
            }
            protected set
            {
                application = value;
                base.Application = application;
            }
        }

        /// <summary>
        /// System check before database operation
        /// 对数据库操作前的系统检查
        /// </summary>
        /// <param name="data">Operation data</param>
        /// <returns>Result</returns>
        override protected OperationResult SystemCheck(OperationData data)
        {
            // Action result
            var result = new OperationResult();

            // User logined?
            if (User.IsAuthenticated)
            {
                // Add system parameters
                SystemUserParameters(data);
                data.Parameters.Add("user_validation_cached", Cached);
                data.Parameters.Add("application_access_token", DBNull.Value);
            }
            else
            {
                // Login required
                result.SetError(-1, "id", "user_login_invalid");
            }

            // Return
            return result;
        }

        /// <summary>
        /// Add system user parameters
        /// 添加系统用户参数
        /// </summary>
        /// <param name="data">Operation data</param>
        override protected void SystemUserParameters(OperationData data)
        {
            data.Parameters.Add("application_role", User.ModuleId);

            if (User.ModuleId == 3)
            {
                data.Parameters.Add("user_application_id", User.Id);
                data.Parameters.Add("application_target", null);
            }
            else
            {
                data.Parameters.Add("user_application_id", Application.Configuration.ServiceUserId);
                data.Parameters.Add("application_target", User.Id);
            }

            data.Parameters.Add("application_language_cid", User.LanguageCid);
        }
    }
}