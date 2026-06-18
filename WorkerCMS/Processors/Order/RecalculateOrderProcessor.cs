using com.etsoo.MessageQueue;
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
        private readonly MyDbContext _db;

        public RecalculateOrderProcessor(ILogger<RecalculateOrderProcessor> logger, LogDbContext logDb, MyDbContext db)
            : base(logger, CrmJsonSerializerContext.Default.RecalculateOrderMessage, logDb)
        {
            _db = db;
        }

        protected override async Task ProcessMessageAsync(RecalculateOrderMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // Organization id
            var orgId = message.Data.OrganizationId;
            if (!orgId.HasValue) return;

            var (orderMonthlyReportEnabled, orderDailyReportHour) = await ProcessorUtils.ReadReportSettingsAsync(_db, orgId.Value, cancellationToken);
            if (!orderMonthlyReportEnabled) return;

            // Order id
            var orderId = message.Data.TargetId;
        }
    }
}
