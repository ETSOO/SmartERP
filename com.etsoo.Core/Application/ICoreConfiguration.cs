namespace com.etsoo.Core.Application
{
    /// <summary>
    /// Application Configuraiton interface
    /// 程序配置接口
    /// </summary>
    public interface ICoreConfiguration
    {
        /// <summary>
        /// Flag for identification, default is 'e', like stored procedure name will start with it
        /// 标识值，默认值为 'e'，比如存储过程会以该字母打头
        /// </summary>
        string Flag { get; }

        /// <summary>
        /// Model DataAnnotations are validated, true under Web API 3 to avoid double validation
        /// 模块数据标记已验证，在Web API 3下可以设置为true以避免重复验证
        /// </summary>
        bool ModelValidated { get; }

        /// <summary>
        /// Private delopyment id, to avoid same encription result with same private key
        /// 私有部署编号，避免多个平台相同加密私匙导致加密结果一样
        /// </summary>
        string PrivateDeploymentId { get; }

        /// <summary>
        /// Private key for encrption
        /// 加密私匙
        /// </summary>
        string PrivateKey { get; }

        /// <summary>
        /// Service user id
        /// 服务用户编号
        /// </summary>
        int ServiceUserId { get; }

        /// <summary>
        /// Symmetric security key, for exchange
        /// 对称安全私匙，用于交换
        /// </summary>
        string SymmetricKey { get; }
    }
}