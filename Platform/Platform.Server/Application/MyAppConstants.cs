namespace Platform.Server.Application
{
    /// <summary>
    /// Service constants
    /// 服务常量
    /// </summary>
    public static class MyAppConstants
    {
        /// <summary>
        /// Core app
        /// 核心程序
        /// </summary>
        public const string CoreApp = "core";

        /// <summary>
        /// Core app id
        /// 核心程序编号
        /// </summary>
        public const int CoreAppId = 1;

        /// <summary>
        /// Admin app
        /// 管理程序
        /// </summary>
        public const string AdminApp = "admin";

        /// <summary>
        /// Admin app id
        /// 管理程序编号
        /// </summary>
        public const int AdminAppId = 2;

        /// <summary>
        /// Registration token audience
        /// 注册令牌受众
        /// </summary>
        public const string RegistrationTokenAudience = "registration";

        /// <summary>
        /// Registration token scheme, using Bearer to share the same AddAuthentication of the API
        /// 注册令牌方案
        /// </summary>
        public const string RegistrationTokenScheme = "Bearer";
    }
}
