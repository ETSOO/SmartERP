namespace CRM.Server.Dto.Customer
{
    /// <summary>
    /// Customer query data
    /// 客户查询数据
    /// </summary>
    public record CustomerQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }
    }
}
