using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person category
    /// 个人类目
    /// </summary>
    public class PersonCategory
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Identity type, employee, customer, or supplier
        /// 标识类型，员工、客户或供应商
        /// </summary>
        public IdentityTypeFlags IdentityType { get; set; }

        /// <summary>
        /// Names
        /// 名称数组
        /// </summary>
        public List<string> Names { get; set; } = default!;

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Creation time
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Parent category id
        /// 父类目编号
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public short OrderIndex { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; set; }
    }
}
