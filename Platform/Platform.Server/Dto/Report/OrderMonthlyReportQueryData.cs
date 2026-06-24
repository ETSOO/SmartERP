namespace Platform.Server.Dto.Report
{
    /// <summary>
    /// Order monthly report query data
    /// 订单月度报表查询数据
    /// </summary>
    public record OrderMonthlyReportQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Period
        /// 区间
        /// </summary>
        public int Period { get; init; }

        /// <summary>
        /// Order Items
        /// 订单项目
        /// </summary>
        public int Items { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Paid amount
        /// 已付款金额
        /// </summary>
        public decimal PaidAmount { get; init; }

        /// <summary>
        /// Discount
        /// 折扣
        /// </summary>
        public decimal Discount { get; init; }

        /// <summary>
        /// Line discount
        /// 行折扣
        /// </summary>
        public decimal LineDiscount { get; init; }

        /// <summary>
        /// Approved discount
        /// 授权折扣
        /// </summary>
        public decimal ApprovedDiscount { get; init; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Customers
        /// 客户数
        /// </summary>
        public int Customers { get; init; }
    }
}
