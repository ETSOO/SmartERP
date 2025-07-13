namespace PlatformShared.Services.ApiOptions
{
    /// <summary>
    /// Storage API options
    /// 存储接口选项
    /// </summary>
    public record StorageApiOptions
    {
        /// <summary>
        /// Provider
        /// 供应商
        /// </summary>
        public string? Provider { get; init; }

        /// <summary>
        /// Root
        /// 根目录
        /// </summary>
        public required string Root { get; init; }

        /// <summary>
        /// URL Root
        /// 网址根目录
        /// </summary>
        public required Uri UrlRoot { get; init; }
    }
}
