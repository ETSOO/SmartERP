using PlatformShared.Dto;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Permission item
    /// 权限项
    /// </summary>
    public class PermissionItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public short Id { get; set; }

        /// <summary>
        /// Module
        /// 模块
        /// </summary>
        public AppModule Module { get; set; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Permission items
        /// 权限项
        /// </summary>
        public ICollection<PersonPermissionItem> PersonPermissionItems { get; } = default!;
    }
}
