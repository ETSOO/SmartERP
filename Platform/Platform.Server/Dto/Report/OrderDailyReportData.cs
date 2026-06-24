namespace Platform.Server.Dto.Report
{
    /// <summary>
    /// Order daily report data
    /// 订单日报表数据
    /// </summary>
    public record OrderDailyReportData
    {
        /// <summary>
        /// Period
        /// 区间
        /// </summary>
        public DateOnly Period { get; init; }

        /// <summary>
        /// Order Items
        /// 订单数
        /// </summary>
        public int Items { get; init; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Customers
        /// 客户数
        /// </summary>
        public int Customers { get; init; }
    }
}
