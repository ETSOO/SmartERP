using com.etsoo.MessageQueue;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Order
{
    /// <summary>
    /// Recalculate order message processor
    /// 重新计算订单消息处理器
    /// </summary>
    public class RecalculateOrderProcessor : LogQueueProcessor<RecalculateOrderMessage>
    {
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly IDbContextFactory<LogDbContext> _logDbFactory;

        public RecalculateOrderProcessor(ILogger<RecalculateOrderProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory, IDbContextFactory<MyDbContext> dbFactory)
            : base(logger, CrmJsonSerializerContext.Default.RecalculateOrderMessage, logDbFactory)
        {
            _dbFactory = dbFactory;
            _logDbFactory = logDbFactory;
        }

        protected override async Task ProcessMessageAsync(RecalculateOrderMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // Organization id
            var orgId = message.Data.OrganizationId;
            if (!orgId.HasValue) return;

            await using var db = _dbFactory.CreateDbContext();
            var (orderMonthlyReportEnabled, orderDailyReportHour, timeZone) = await ProcessorUtils.ReadReportSettingsAsync(db, orgId.Value, cancellationToken);
            if (!orderMonthlyReportEnabled) return;

            await using var logDb = await _logDbFactory.CreateDbContextAsync(cancellationToken);

            // Add it to the daily report
            if (orderDailyReportHour.HasValue)
            {
                // Daily report
                await ProcessorUtils.CreateOrderDailyReportAsync(db, logDb, orgId.Value, orderDailyReportHour.Value, message.OrderCreation, timeZone, cancellationToken);
            }
            else
            {
                // Monthly report
                await ProcessorUtils.CreateOrderMonthlyReportAsync(db, logDb, orgId.Value, 0, message.OrderCreation, timeZone, cancellationToken);
            }
        }
    }
}
