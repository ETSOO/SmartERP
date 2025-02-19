using com.etsoo.CoreFramework.Business;
using PlatformShared.Dto;

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
        /// Local name
        /// 本地名称
        /// </summary>
        public string? LocalName { get; init; }

        /// <summary>
        /// Identity type
        /// 身份类型
        /// </summary>
        public IdentityType IdentityType { get; init; }

        /// <summary>
        /// URLs
        /// 网址
        /// </summary>
        public required AppUrl[] Urls { get; init; }

        /// <summary>
        /// Require local URL or not
        /// 是否需要本地地址
        /// </summary>
        public bool? RequireLocalUrl { get; init; }

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; init; }
    }
}
