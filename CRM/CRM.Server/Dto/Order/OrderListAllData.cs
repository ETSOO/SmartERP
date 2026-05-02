namespace CRM.Server.Dto.Order
{
    /// <summary>
    /// Order / PO list data
    /// 订单采购列表数据
    /// </summary>
    public record OrderListAllData
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
        /// Is order or not
        /// 是否为订单
        /// </summary>
        public bool IsOrder { get; init; }
    }
}
