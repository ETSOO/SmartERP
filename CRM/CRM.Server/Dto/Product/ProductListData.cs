namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Product list data
    /// 产品列表数据
    /// </summary>
    public record ProductListData
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
        /// Base unit
        /// 基础单位
        /// </summary>
        public com.etsoo.CoreFramework.Business.ProductUnit BaseUnit { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }
    }
}
