using com.etsoo.CoreFramework.Business;
using PlatformShared.Database.Models;
using PlatformShared.Dto;

namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Product query data
    /// 产品查询数据
    /// </summary>
    public record ProductQueryData
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
        /// Asset qty
        /// 资产数量
        /// </summary>
        public int? AssetQty { get; init; }

        /// <summary>
        /// Sale scope
        /// 销售范围
        /// </summary>
        public ProductScope Scope { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string? Currency {  get; init; }

        /// <summary>
        /// Retail price
        /// 零售价
        /// </summary>
        public decimal? RetailPrice { get; init; }

        /// <summary>
        /// Promotion price
        /// 促销价
        /// </summary>
        public decimal? PromotionPrice { get; init; }

        /// <summary>
        /// Unit name
        /// 单位名称
        /// </summary>
        public required string UnitName { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<CategoryItem>? Categories { get; init; }
    }
}
