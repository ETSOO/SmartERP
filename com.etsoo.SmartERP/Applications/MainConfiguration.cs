using com.etsoo.Core.Application;

namespace com.etsoo.SmartERP.Applications
{
    /// <summary>
    /// Main configuration
    /// 扩展的配置
    /// </summary>
    public class MainConfiguration : ConfigurationBase
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        public MainConfiguration()
        {
            // Default languages
            this.Languages = new string[] { "zh-CN", "en-US" };
        }

        /// <summary>
        /// Main configuration builder
        /// 扩展的配置器
        /// </summary>
        public class Builder : BuilderBase<MainConfiguration>
        {
            /// <summary>
            /// Constructor
            /// 构造函数
            /// </summary>
            public Builder() : base(new MainConfiguration())
            {
            }
        }
    }
}