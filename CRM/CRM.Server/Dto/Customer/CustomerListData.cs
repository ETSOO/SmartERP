namespace CRM.Server.Dto.Customer
{
    /// <summary>
    /// Customer list data
    /// 客户列表数据
    /// </summary>
    public record CustomerListData
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
