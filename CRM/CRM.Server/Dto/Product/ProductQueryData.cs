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
    }
}
