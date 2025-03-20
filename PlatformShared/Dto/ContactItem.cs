using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Contact item
    /// 联系人项
    /// </summary>
    public record ContactItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Identity type, employee, customer, or supplier
        /// 标识类型，员工、客户或供应商
        /// </summary>
        public IdentityTypeFlags? IdentityType { get; set; }

        /// <summary>
        /// Preferred name
        /// 首先名
        /// </summary>
        public string? PreferredName { get; set; }

        /// <summary>
        /// Is organization self
        /// 是否为机构本身
        /// </summary>
        public bool IsOrg { get; init; }
    }
}
