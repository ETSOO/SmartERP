using com.etsoo.MessageQueue;
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
        private readonly MyDbContext _db;

        public CreateOrderProcessor(ILogger<CreateOrderProcessor> logger, LogDbContext logDb, MyDbContext db)
            : base(logger, CrmJsonSerializerContext.Default.CreateOrderMessage, logDb)
        {
            _db = db;
        }

        protected override async Task ProcessMessageAsync(CreateOrderMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // Organization id
            var orgId = message.Data.OrganizationId;
            if (!orgId.HasValue) return;

            var (orderMonthlyReportEnabled, orderDailyReportHour) = await ProcessorUtils.ReadReportSettingsAsync(_db, orgId.Value, cancellationToken);
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
