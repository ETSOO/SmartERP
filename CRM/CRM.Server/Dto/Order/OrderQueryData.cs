namespace CRM.Server.Dto.Order
{
    /// <summary>
    /// Order query data
    /// 订单查询数据
    /// </summary>
    public record OrderQueryData
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
