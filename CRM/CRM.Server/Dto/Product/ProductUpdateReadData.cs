using com.etsoo.CoreFramework.Business;
using PlatformShared.Database.Models;

namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Product update read data
    /// 更新产品读取数据
    /// </summary>
    public record ProductUpdateReadData
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
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Unit id
        /// 产品单位编号
        /// </summary>
        public int UnitId { get; init; }

        /// <summary>
        /// Minimum purchase qty
        /// 最少购买量
        /// </summary>
        public decimal? MinQty { get; init; }

        /// <summary>
        /// Purchase minimum unit
        /// 购买最小单位
        /// </summary>
        public decimal? StepQty { get; init; }

        /// <summary>
        /// Maximum purchase qty
        /// 最大购买量
        /// </summary>
        public decimal? CapQty { get; init; }

        /// <summary>
        /// Asset qty
        /// 资产数量
        /// </summary>
        public int? AssetQty { get; init; }

        /// <summary>
        /// Usage
        /// 使用范围
        /// </summary>
        public ProductUsage Usage { get; init; }

        /// <summary>
        /// Sale scope
        /// 销售范围
        /// </summary>
        public ProductScope Scope { get; init; }

        /// <summary>
        /// Inventory management way
        /// 库存管理方式
        /// </summary>
        public ProductInventoryWay InventoryWay { get; init; }

        /// <summary>
        /// Query keyword
        /// 查询关键词
        /// </summary>
        public string? QueryKeyword { get; init; }

        /// <summary>
        /// Price
        /// 价格
        /// </summary>
        public ProductPriceItem? Price { get; init; }

        /// <summary>
        /// Tax rate
        /// 税率
        /// </summary>
        public decimal? TaxRate { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<int>? Categories { get; init; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        public IEnumerable<string>? Tags { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }
    }
}
