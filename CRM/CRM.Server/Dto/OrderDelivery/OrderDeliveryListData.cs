namespace CRM.Server.Dto.OrderDelivery
{
    /// <summary>
    /// Order delivery list data
    /// 订单配送方式列表数据
    /// </summary>
    public record OrderDeliveryListData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }
    }
}
