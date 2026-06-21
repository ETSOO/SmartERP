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

        public RecalculateOrderProcessor(ILogger<RecalculateOrderProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory, IDbContextFactory<MyDbContext> dbFactory)
            : base(logger, CrmJsonSerializerContext.Default.RecalculateOrderMessage, logDbFactory)
        {
            _dbFactory = dbFactory;
        }

        protected override async Task ProcessMessageAsync(RecalculateOrderMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // Organization id
            var orgId = message.Data.OrganizationId;
            if (!orgId.HasValue) return;

            await using var db = _dbFactory.CreateDbContext();
            var (orderMonthlyReportEnabled, orderDailyReportHour) = await ProcessorUtils.ReadReportSettingsAsync(db, orgId.Value, cancellationToken);
            if (!orderMonthlyReportEnabled) return;

            // Order id
            var orderId = message.Data.TargetId;

            // Add it to the daily report
            if (orderDailyReportHour.HasValue)
            {

            }

            // Add it to the monthly report
            if (orderDailyReportHour.HasValue)
            {
                // Summary from daily report
            }
            else
            {

            }
        }
    }
}
