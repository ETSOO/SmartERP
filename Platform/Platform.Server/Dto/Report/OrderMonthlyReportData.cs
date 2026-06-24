namespace Platform.Server.Dto.Report
{
    /// <summary>
    /// Order monthly report data
    /// 订单月报表数据
    /// </summary>
    public record OrderMonthlyReportData
    {
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
        /// Customers
        /// 客户数
        /// </summary>
        public int Customers { get; init; }
    }
}
