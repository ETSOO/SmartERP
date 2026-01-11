namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Product duplicate test data
    /// 产品重复测试数据
    /// </summary>
    public record ProductDuplicateTestData
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
    }
}
