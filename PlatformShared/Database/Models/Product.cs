using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Product usage
    /// 产品用途
    /// </summary>
    public enum ProductUsage : byte
    {

    }

    /// <summary>
    /// Product sales control scope
    /// 产品销售控制范围
    /// </summary>
    public enum ProductScope : byte
    {
    }

    /// <summary>
    /// Product inventory management way
    /// 产品库存管理方式
    /// </summary>
    public enum ProductInventoryWay : byte
    {
        /// <summary>
        /// None
        /// 无
        /// </summary>
        None = 0,

        /// <summary>
        /// Simple
        /// 简单管理
        /// </summary>
        Simple = 4,

        /// <summary>
        /// Full
        /// 完整管理
        /// </summary>
        Full = 9
    }

    /// <summary>
    /// Product
    /// 产品
    /// </summary>
    public class Product
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
        /// Name
        /// 名称
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Foreign name
        /// 外文名称
        /// </summary>
        public string? ForeignName { get; set; }

        /// <summary>
        /// Category ids
        /// 所属类目编号数组
        /// </summary>
        public List<int> CategoryIds { get; set; } = default!;

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; set; }

        /// <summary>
        /// Unit id
        /// 产品单位编号
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// Minimum purchase qty
        /// 最少购买量
        /// </summary>
        public decimal? MinQty { get; set; }

        /// <summary>
        /// Purchase minimum unit
        /// 购买最小单位
        /// </summary>
        public decimal? StepQty { get; set; }

        /// <summary>
        /// Maximum purchase qty
        /// 最大购买量
        /// </summary>
        public decimal? CapQty { get; set; }

        /// <summary>
        /// Asset unit
        /// 资产单位
        /// </summary>
        public AssetUnit? AssetUnit { get; set; }

        /// <summary>
        /// Asset qty
        /// 资产数量
        /// </summary>
        public int? AssetQty { get; set; }

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public short OrderIndex { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTime Creation { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ProductUsage Usage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ProductScope Scope { get; set; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Query keyword
        /// 查询关键词
        /// </summary>
        public string? QueryKeyword { get; set; }

        /// <summary>
        /// Inventory management way
        /// 库存管理方式
        /// </summary>
        public ProductInventoryWay InventoryWay { get; set; }

        /// <summary>
        /// Tags (id)
        /// 标签（编号）
        /// </summary>
        public List<int>? Tags { get; set; }
    }
}
