using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Product usage
    /// 产品用途
    /// </summary>
    public enum ProductUsage : byte
    {
        /// <summary>
        /// Raw material
        /// 原材料
        /// </summary>
        RawMaterial = 1,

        /// <summary>
        /// Work-in-progress
        /// 半成品
        /// </summary>
        WIP = 4,

        /// <summary>
        /// Finished product
        /// 成品
        /// </summary>
        FinishedProduct = 9
    }

    /// <summary>
    /// Product sales control scope
    /// 产品销售控制范围
    /// </summary>
    public enum ProductScope : byte
    {
        /// <summary>
        /// None
        /// 无
        /// </summary>
        None = 0,

        /// <summary>
        /// Internal
        /// 仅内部
        /// </summary>
        Internal = 1,

        /// <summary>
        /// Public
        /// 仅对外
        /// </summary>
        Public = 16
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
        /// Category ids
        /// 所属类目编号数组
        /// </summary>
        public List<int>? CategoryIds { get; set; }

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
        /// Introduction Url
        /// 介绍链接
        /// </summary>
        public string? IntroductionUrl { get; set; }

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
        /// Asset qty
        /// 资产数量
        /// </summary>
        public int? AssetQty { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配编号
        /// </summary>
        public string? AssignedId { get; set;  }

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public short OrderIndex { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; }

        /// <summary>
        /// Usage
        /// 使用范围
        /// </summary>
        public ProductUsage Usage { get; set; }

        /// <summary>
        /// Sale scope
        /// 销售范围
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
        /// Tax rate
        /// 税率
        /// </summary>
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Tags (id)
        /// 标签（编号）
        /// </summary>
        public List<int>? Tags { get; set; }

        /// <summary>
        /// Organization
        /// 所属机构
        /// </summary>
        public CoreOrganization CoreOrganization { get; set; } = null!;

        /// <summary>
        /// Product unit
        /// 产品单位
        /// </summary>
        public ProductUnit Unit { get; set; } = null!;

        /// <summary>
        /// Assets
        /// 资产
        /// </summary>
        public ICollection<PersonAsset> Assets { get; set; } = default!;

        /// <summary>
        /// Prices
        /// 价格
        /// </summary>
        public ICollection<ProductPrice> Prices { get; set; } = default!;
    }
}
