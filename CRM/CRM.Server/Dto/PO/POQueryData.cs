namespace CRM.Server.Dto.PO
{
    /// <summary>
    /// Purchase order query data
    /// 采购查询数据
    /// </summary>
    public record POQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }
    }
}
