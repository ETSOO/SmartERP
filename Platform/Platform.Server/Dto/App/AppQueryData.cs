using com.etsoo.CoreFramework.Business;

namespace Platform.Server.Dto.App
{
    /// <summary>
    /// Application query data
    /// 应用查询数据
    /// </summary>
    public record AppQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Identity type
        /// 身份类型
        /// </summary>
        public IdentityType IdentityType { get; init; }

        /// <summary>
        /// Require local URL or not
        /// 是否需要本地地址
        /// </summary>
        public bool? RequireLocalUrl { get; init; }

        /// <summary>
        /// Web URL
        /// 网页地址
        /// </summary>
        public required string WebUrl { get; init; }

        /// <summary>
        /// Help URL
        /// 帮助地址
        /// </summary>
        public string? HelpUrl { get; init; }

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; init; }
    }
}
