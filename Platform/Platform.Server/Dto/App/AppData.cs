using PlatformShared.Dto;

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
        /// Local name
        /// 本地名称
        /// </summary>
        public string? LocalName { get; init; }

        /// <summary>
        /// URLs
        /// 网址
        /// </summary>
        public required AppUrl[] Urls { get; init; }

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; init; }
    }
}
