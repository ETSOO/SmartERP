namespace WorkerCenter.Periods
{
    /// <summary>
    /// Options for the daily worker
    /// 每日处理程序选项
    /// </summary>
    public record DailyWorkerOptions
    {
        /// <summary>
        /// Cron expression for the daily worker
        /// </summary>
        public required string Cron { get; init; }
    }
}
