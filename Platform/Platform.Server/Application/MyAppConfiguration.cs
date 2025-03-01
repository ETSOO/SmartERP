using com.etsoo.CoreFramework.Application;

namespace Platform.Server.Application
{
    /// <summary>
    /// My application configuration
    /// 本地程序配置
    /// </summary>
    public record MyAppConfiguration : AppConfiguration
    {
        /// <summary>
        /// Authentication failure URL
        /// 失败认证URL
        /// </summary>
        public required string AuthFailureUrl { get; set; }

        /// <summary>
        /// Authentication registration URL
        /// 成功注册URL
        /// </summary>
        public required string AuthRegistrationUrl { get; set; }

        /// <summary>
        /// Authentication success URL
        /// 成功认证URL
        /// </summary>
        public required string AuthSuccessUrl { get; set; }

        /// <summary>
        /// Super admin organization ID
        /// 超级管理员机构编号
        /// </summary>
        public required int SuperAdminOrganizationId { get; set; }

        /// <summary>
        /// Core app auth API URL
        /// 中心应用授权接口URL
        /// </summary>
        public required string CoreAppAuthApiUrl { get; set; }
    }
}
