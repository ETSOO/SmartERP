namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Product unit
    /// 产品单位
    /// </summary>
    public class ProductUnit
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
        public int? CoreOrganizationId { get; set; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Base unit
        /// 基础单位
        /// </summary>
        public com.etsoo.CoreFramework.Business.ProductUnit BaseUnit { get; set; } = default!;

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public short OrderIndex { get; set; }

        /// <summary>
        /// Organization
        /// 所属机构
        /// </summary>
        public CoreOrganization? CoreOrganization { get; set; }

        /// <summary>
        /// Products
        /// 关联产品
        /// </summary>
        public ICollection<Product> Products { get; set; } = default!;
    }
}
