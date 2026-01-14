namespace CRM.Server.Dto.ProductCategory
{
    /// <summary>
    /// Product category query data
    /// 产品分类查询数据
    /// </summary>
    public record ProductCategoryQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Names
        /// 名称列表
        /// </summary>
        public required IEnumerable<string> Names { get; init; }

        /// <summary>
        /// Assigned ID
        /// 分配编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
