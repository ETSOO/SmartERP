using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Product BOM
    /// 产品物料清单
    /// </summary>
    public class ProductBom
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Parent product id
        /// 父产品编号
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// Parent product
        /// 父产品
        /// </summary>
        public Product Parent { get; set; } = default!;

        /// <summary>
        /// Product
        /// 产品
        /// </summary>
        public Product Product { get; set; } = default!;
    }
}
