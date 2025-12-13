using com.etsoo.CoreFramework.Application;
using System.ComponentModel.DataAnnotations;

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
        [Url]
        public string AuthFailureUrl { get; set; } = default!;

        /// <summary>
        /// Authentication registration URL
        /// 成功注册URL
        /// </summary>
        [Url]
        public string AuthRegistrationUrl { get; set; } = default!;

        /// <summary>
        /// Authentication success URL
        /// 成功认证URL
        /// </summary>
        [Url]
        public string AuthSuccessUrl { get; set; } = default!;

        /// <summary>
        /// Super admin organization ID
        /// 超级管理员机构编号
        /// </summary>
        public int SuperAdminOrganizationId { get; set; }

        /// <summary>
        /// Core app auth API URL
        /// 中心应用授权接口URL
        /// </summary>
        [Url]
        public string CoreAppAuthApiUrl { get; set; } = default!;
    }
}
