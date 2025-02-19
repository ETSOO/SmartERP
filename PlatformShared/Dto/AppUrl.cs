namespace PlatformShared.Dto
{
    /// <summary>
    /// Application URL
    /// 应用网址
    /// </summary>
    public record AppUrl
    {
        /// <summary>
        /// Web URL
        /// </summary>
        public required string Web { get; init; }

        /// <summary>
        /// API URL
        /// </summary>
        public required string Api { get; init; }

        /// <summary>
        /// Help URL
        /// </summary>
        public string? Help { get; init; }
    }
}
