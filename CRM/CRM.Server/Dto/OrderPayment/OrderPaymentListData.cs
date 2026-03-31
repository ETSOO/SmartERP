namespace CRM.Server.Dto.OrderPayment
{
    /// <summary>
    /// Order payment list data
    /// 订单支付方式列表数据
    /// </summary>
    public record OrderPaymentListData
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
