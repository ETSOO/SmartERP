namespace PlatformShared.Services
{
    /// <summary>
    /// SmartERP Coordinator options
    /// 司友云ERP协调选项
    /// </summary>
    public record SmartERPCoordinatorOptions
    {
        /// <summary>
        /// Private key
        /// 私钥
        /// </summary>
        public required string PrivateKey { get; init; }
    }
}
