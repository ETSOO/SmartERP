namespace Platform.Server.Dto.App
{
    /// <summary>
    /// Application data
    /// 程序数据
    /// </summary>
    public record AppData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// Application name
        /// 程序名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Web URL
        /// Web网址
        /// </summary>
        public required string WebUrl { get; init; }

        /// <summary>
        /// Help URL
        /// 帮助网址
        /// </summary>
        public string? HelpUrl { get; init; }

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; init; }
    }
}
