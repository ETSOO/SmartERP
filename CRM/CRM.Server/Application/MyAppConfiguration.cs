using com.etsoo.ServiceApp.Application;

namespace CRM.Server.Application
{
    /// <summary>
    /// My application configuration
    /// 本地程序配置
    /// </summary>
    public record MyAppConfiguration : ServiceAppConfiguration
    {
        /// <summary>
        /// Authentication clients
        /// "Wechat", "Alipay", "Google", "Microsoft"
        /// 授权客户端
        /// </summary>
        public string[] AuthClients { get; set; } = [];
    }
}
