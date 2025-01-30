using com.etsoo.MessageQueue;
using Microsoft.Extensions.Options;

namespace WorkerCenter.Periods
{
    public class DailyWorker : SchedulerBackgroundService
    {
        private readonly ILogger<DailyWorker> _logger;
        private readonly DailyWorkerOptions _options;

        public DailyWorker(ILogger<DailyWorker> logger,
            IOptions<DailyWorkerOptions> options) : base(options.Value.Cron)
        {
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Daily worker is running");
        }
    }
}
