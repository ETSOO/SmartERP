namespace WorkerCenter.Workers
{
    /// <summary>
    /// Options for the worker
    /// 处理程序选项
    /// </summary>
    internal record WorkerOptions
    {
        /// <summary>
        /// Cron expression for the worker
        /// */5 * * * * means every 5 minutes
        /// 30 1 * * * means every day at 1:30 am
        /// Cron 表达式，用于定义处理程序的调度时间
        /// </summary>
        public required string Cron { get; set; }
    }
}
