namespace Platform.Server.Dto.Report
{
    /// <summary>
    /// Title report data
    /// 标题报表数据
    /// </summary>
    public record TitleReportData
    {
        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Value
        /// 值
        /// </summary>
        public decimal Value { get; init; }
    }
}
