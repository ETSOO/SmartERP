using com.etsoo.MessageQueue;
using com.etsoo.Utils;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Create order message processor
    /// 创建订单消息处理器
    /// </summary>
    public class CreateOrderProcessor : LogQueueProcessor<CreateOrderMessage>
    {
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly IDbContextFactory<LogDbContext> _logDbFactory;
        private readonly Debouncer<int> _debouncer;

        public CreateOrderProcessor(ILogger<CreateOrderProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory, IDbContextFactory<MyDbContext> dbFactory)
            : base(logger, CrmJsonSerializerContext.Default.CreateOrderMessage, logDbFactory)
        {
            _dbFactory = dbFactory;
            _logDbFactory = logDbFactory;
            _debouncer = new Debouncer<int>(DebouncerActionAsync, TimeSpan.FromMinutes(3));
        }

        private async Task DebouncerActionAsync(int orgId, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var (orderMonthlyReportEnabled, orderDailyReportHour, timeZone) = await ProcessorUtils.ReadReportSettingsAsync(db, orgId, cancellationToken);
            if (!orderMonthlyReportEnabled) return;

            await using var logDb = await _logDbFactory.CreateDbContextAsync(cancellationToken);

            if (orderDailyReportHour.HasValue)
            {
                // Daily report
                await ProcessorUtils.CreateOrderDailyReportAsync(db, logDb, orgId, orderDailyReportHour.Value, DateTimeOffset.UtcNow, timeZone, cancellationToken);
            }
            else
            {
                // Monthly report
                await ProcessorUtils.CreateOrderMonthlyReportAsync(db, logDb, orgId, 0, DateTimeOffset.UtcNow, timeZone, cancellationToken);
            }
        }

        protected override async Task ProcessMessageAsync(CreateOrderMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // Organization id
            var orgId = message.Data.OrganizationId;
            if (!orgId.HasValue) return;

            // Update debouncer
            _debouncer.Debounce(orgId.Value);
        }

        ~CreateOrderProcessor()
        {
            _debouncer.Dispose();
        }
    }
}
