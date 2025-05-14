namespace CRM.Server.Dto.Order
{
    /// <summary>
    /// Order list data
    /// 订单列表数据
    /// </summary>
    public record OrderListData
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
