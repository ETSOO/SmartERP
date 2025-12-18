namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Product category
    /// 产品类目
    /// </summary>
    public class ProductCategory
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Names
        /// 名称数组
        /// </summary>
        public List<string> Names { get; set; } = default!;

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; set; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Parent id
        /// 上级类目编号
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Parent Ids
        /// 所有父类编号
        /// </summary>
        public List<int>? ParentIds { get; set; }

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public short OrderIndex { get; set; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; set; }
    }
}
