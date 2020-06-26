namespace com.etsoo.Api.Helpers
{
    /// <summary>
    /// SmartERP Settings Model
    /// SmartERP设置模型
    /// </summary>
    public class SmartERPSettings
    {
        /// <summary>
        /// Connection string id, comply with ConnectionStrings/{id} database configuration node
        /// 链接字符串编号，遵守配置节点ConnectionStrings下的数据库链接配置
        /// </summary>
        public string ConnectionStringId { get; set; }

        /// <summary>
        /// CORS (Cross-Origin Requests) allowed sites, support similar *.etsoo.com subdomains
        /// 跨域访问配置允许的站点，支持通用子域名形式
        /// </summary>
        public string[] Cors { get; set; }

        /// <summary>
        /// Private key for encryption
        /// 加密私匙
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// Service user id
        /// 服务用户编号
        /// </summary>
        public int ServiceUserId { get; set; }

        /// <summary>
        /// Is SSL only
        /// 是否仅限于SSL访问
        /// </summary>
        public bool SSL { get; set; }

        /// <summary>
        /// Symmetric security key, for exchange
        /// 对称安全私匙，用于交换
        /// </summary>
        public string SymmetricKey { get; set; }
    }
}
