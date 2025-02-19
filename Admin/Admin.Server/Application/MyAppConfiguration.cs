using com.etsoo.ServiceApp.Application;

namespace Admin.Server.Application
{
    /// <summary>
    /// My application configuration
    /// 本地程序配置
    /// </summary>
    public record MyAppConfiguration : ServiceAppConfiguration
    {
        /// <summary>
        /// Super admin organization ID
        /// 超级管理员机构编号
        /// </summary>
        public required int SuperAdminOrganizationId { get; set; }
    }
}
