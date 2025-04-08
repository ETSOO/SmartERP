namespace PlatformShared.Dto
{
    /// <summary>
    /// File data
    /// 文件数据
    /// </summary>
    public record FileData
    {
        /// <summary>
        /// File name
        /// 文件名
        /// </summary>
        public required string FileName { get; init; }

        /// <summary>
        /// Content type
        /// 文件类型
        /// </summary>
        public required string ContentType { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }
    }
}
