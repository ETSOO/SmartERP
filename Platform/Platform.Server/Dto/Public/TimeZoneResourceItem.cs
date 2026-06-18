namespace Platform.Server.Dto.Public
{
    /// <summary>
    /// Time zone resource item
    /// 时区资源项目
    /// </summary>
    public record TimeZoneResourceItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Label
        /// 标签
        /// </summary>
        public string? Label { get; init; }
    }
}
