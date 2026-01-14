using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Product unit item
    /// 产品单位项
    /// </summary>
    public record ProductUnitItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int? Id { get; init; }

        /// <summary>
        /// Base unit
        /// 基本单位
        /// </summary>
        public ProductUnit BaseUnit { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Is system item
        /// 是否为系统项
        /// </summary>
        public bool IsSystem { get; init; }
    }
}
