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
        private readonly Debouncer<int> debouncer;

        public CreateOrderProcessor(ILogger<CreateOrderProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory, IDbContextFactory<MyDbContext> dbFactory)
            : base(logger, CrmJsonSerializerContext.Default.CreateOrderMessage, logDbFactory)
        {
            _dbFactory = dbFactory;
            debouncer = new Debouncer<int>(DebouncerActionAsync);
        }

        private async Task DebouncerActionAsync(int orgId, CancellationToken cancellationToken)
        {
            await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var (orderMonthlyReportEnabled, orderDailyReportHour) = await ProcessorUtils.ReadReportSettingsAsync(_db, orgId, cancellationToken);
            if (!orderMonthlyReportEnabled) return;
        }

        protected override async Task ProcessMessageAsync(CreateOrderMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // Organization id
            var orgId = message.Data.OrganizationId;
            if (!orgId.HasValue) return;

            // Update debouncer, 3 mins
            debouncer.Debounce(orgId.Value, TimeSpan.FromMinutes(3));
        }

        ~CreateOrderProcessor()
        {
            debouncer.Dispose();
        }
    }
}
