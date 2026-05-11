namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Product BOM item
    /// 产品物料清单项
    /// </summary>
    public record ProductBomItem
    {
        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }
    }

    /// <summary>
    /// Product BOM name item
    /// 产品物料清单名称项
    /// </summary>
    public record ProductBomNameItem : ProductBomItem
    {
        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }
    }
}
