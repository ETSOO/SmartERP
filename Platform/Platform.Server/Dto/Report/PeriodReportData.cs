namespace Platform.Server.Dto.Report
{
    /// <summary>
    /// Period report data
    /// 周期报表数据
    /// </summary>
    public record PeriodReportData
    {
        /// <summary>
        /// Period
        /// 周期
        /// </summary>
        public int Period { get; init; }

        /// <summary>
        /// Value
        /// 值
        /// </summary>
        public decimal Value { get; init; }
    }
}