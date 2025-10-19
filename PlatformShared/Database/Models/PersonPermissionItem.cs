namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person permission item
    /// 人员权限项目
    /// </summary>
    public class PersonPermissionItem
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        /// Permission item id
        /// 权限项目编号
        /// </summary>
        public short PermissionItemId { get; set; }

        /// <summary>
        /// Permission item
        /// 权限项目
        /// </summary>
        public PermissionItem PermissionItem { get; } = default!;

        /// <summary>
        /// Person
        /// 人员
        /// </summary>
        public Person Person { get; } = default!;
    }
}
