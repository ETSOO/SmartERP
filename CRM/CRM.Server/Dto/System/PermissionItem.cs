using PlatformShared.Dto;

namespace CRM.Server.Dto.System
{
    /// <summary>
    /// Permission item
    /// 权限项
    /// </summary>
    public record PermissionItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public short Id { get; init; }

        /// <summary>
        /// Module
        /// 模块
        /// </summary>
        public AppModule Module { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }
    }
}
