namespace com.etsoo.Core.Application
{
    /// <summary>
    /// Common configuration for application
    /// 程序对象的通用配置
    /// </summary>
    public abstract class ConfigurationBase : ICoreConfiguration
    {
        /// <summary>
        /// Fluent builder base
        /// 液态基础构建器
        /// </summary>
        public abstract class BuilderBase<T> where T : ConfigurationBase
        {
            /// <summary>
            /// Configuration base
            /// 基础配置对象
            /// </summary>
            private T Configuration;

            /// <summary>
            /// Constructor
            /// 构造函数
            /// </summary>
            /// <param name="configuration">Configuration</param>
            protected BuilderBase(T configuration)
            {
                Configuration = configuration;
            }

            /// <summary>
            /// Build the configuration
            /// 创建配置对象
            /// </summary>
            /// <returns>Configuration</returns>
            public T Build()
            {
                return Configuration;
            }

            /// <summary>
            /// Set supported languages
            /// 设置支持的语言
            /// </summary>
            /// <param name="languages">Languages</param>
            /// <returns>Builder</returns>
            public BuilderBase<T> Languages(string[] languages)
            {
                Configuration.Languages = languages;
                return this;
            }

            /// <summary>
            /// Whether model is validated
            /// 模块是否已经通过验证
            /// </summary>
            /// <param name="validated">Validated</param>
            /// <returns>Builder</returns>
            public BuilderBase<T> ModelValidated(bool validated)
            {
                Configuration.ModelValidated = validated;
                return this;
            }

            /// <summary>
            /// Set private deployment
            /// 设置私有化部署
            /// </summary>
            /// <param name="id">Private delopyment id, to avoid same encription result with same private key</param>
            /// <returns>Builder</returns>
            public BuilderBase<T> PrivateDeployment(string id)
            {
                Configuration.PrivateDeploymentId = id;
                return this;
            }

            /// <summary>
            /// Set service user
            /// 设置服务账号
            /// </summary>
            /// <param name="id">Service user id</param>
            /// <returns>Builder</returns>
            public BuilderBase<T> ServiceUser(int id)
            {
                Configuration.ServiceUserId = id;
                return this;
            }

            /// <summary>
            /// Set keys
            /// 设置键
            /// </summary>
            /// <param name="privateKey">Private key for encrption</param>
            /// <param name="symmetricKey">Symmetric security key, for exchange</param>
            /// <returns>Builder</returns>
            public BuilderBase<T> SetKeys(string privateKey, string symmetricKey)
            {
                Configuration.PrivateKey = privateKey;
                Configuration.SymmetricKey = symmetricKey;
                return this;
            }
        }

        /// <summary>
        /// Flag for identification, default is 'e', like stored procedure name will start with it
        /// 标识值，默认值为 'e'，比如存储过程会以该字母打头
        /// </summary>
        public virtual string Flag
        {
            get { return "e"; }
        }

        /// <summary>
        /// Supported languages
        /// 支持的语言
        /// </summary>
        public string[] Languages { get; protected set; }

        /// <summary>
        /// Model DataAnnotations are validated, true under Web API 3 to avoid double validation
        /// 模块数据标记已验证，在Web API 3下可以设置为true以避免重复验证
        /// </summary>
        public bool ModelValidated { get; private set; }

        private string privateDeploymentId;
        /// <summary>
        /// Private delopyment id, to avoid same encription result with same private key
        /// 私有部署编号，避免多个平台相同加密私匙导致加密结果一样
        /// </summary>
        public string PrivateDeploymentId
        {
            get { return privateDeploymentId; }
            private set
            {
                if (value == null)
                    value = string.Empty;

                privateDeploymentId = value;
            }
        }

        /// <summary>
        /// Private key for encrption
        /// 加密私匙
        /// </summary>
        public string PrivateKey { get; private set; }

        /// <summary>
        /// Service user id
        /// 服务用户编号
        /// </summary>
        public int ServiceUserId { get; private set; }

        /// <summary>
        /// Symmetric security key, for exchange
        /// 对称安全私匙，用于交换
        /// </summary>
        public string SymmetricKey { get; private set; }
    }
}