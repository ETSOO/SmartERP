namespace CRM.Server.Dto.OrderLine
{
    /// <summary>
    /// Order line list data
    /// 订单行列表数据
    /// </summary>
    public record OrderLineListData
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

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }
    }
}
