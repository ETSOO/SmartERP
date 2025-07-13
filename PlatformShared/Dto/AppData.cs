namespace PlatformShared.Dto
{
    /// <summary>
    /// Application data
    /// 应用数据
    /// </summary>
    public record AppData
    {
        /// <summary>
        /// App secret
        /// 应用密钥
        /// </summary>
        public required string AppSecret { get; set; }

        /// <summary>
        /// URLs
        /// 网址
        /// </summary>
        public required AppUrl[] Urls { get; init; }
    }
}