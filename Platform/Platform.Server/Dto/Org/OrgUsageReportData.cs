namespace Platform.Server.Dto.Org
{
    /// <summary>
    /// Organization usage report data
    /// 机构使用报告数据
    /// </summary>
    public record OrgUsageReportData
    {
        /// <summary>
        /// Period
        /// 周期
        /// </summary>
        public int Period { get; init; }

        /// <summary>
        /// Qty.
        /// 数量
        /// </summary>
        public int Qty { get; init; }
    }
}
