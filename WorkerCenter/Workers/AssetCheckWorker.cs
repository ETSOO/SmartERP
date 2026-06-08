using com.etsoo.MessageQueue;
using PlatformShared.Database;

namespace WorkerCenter.Periods
{
    internal class AssetCheckWorker : BackgroundService
    {
        private readonly MyDbContext _db;
        private readonly ILogger<AssetCheckWorker> _logger;
        private readonly IMessageQueueProducer _producer;

        public AssetCheckWorker(
            MyDbContext db,
            ILogger<AssetCheckWorker> logger,
            IMessageQueueProducer producer
        )
        {
            _db = db;
            _logger = logger;
            _producer = producer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait for 1 minute before starting the loop
            await Task.Delay(60000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {

            }
        }
    }
}
