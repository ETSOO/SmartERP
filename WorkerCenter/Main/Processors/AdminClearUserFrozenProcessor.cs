using com.etsoo.Localization;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.LogDatabase.Models;
using PlatformShared.Messages;
using System.Net;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Admin clear user frozon time processor
    /// 管理员清除用户冻结时间处理器
    /// </summary>
    public class AdminClearUserFrozenProcessor : CommonQueueProcessor<AdminClearUserFrozenMessage>
    {
        private readonly LogDbContext _logDb;
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public AdminClearUserFrozenProcessor(ILogger<AdminClearUserFrozenProcessor> logger,
            LogDbContext logDb,
            MyDbContext db,
            IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.AdminClearUserFrozenMessage)
        {
            _producer = producer;
            _logDb = logDb;
            _db = db;
        }

        private async Task LogAsync(AdminClearUserFrozenMessage message, int userId, int? orgId, CancellationToken cancellationToken)
        {
            var data = message.Data;
            var title = $"{Properties.Resources.AdminClearUserFrozen} ({data.TargetName})";

            var log = new CoreLog
            {
                Culture = data.Culture,
                Data = message.GetMoreData(),
                DeviceId = data.DeviceId,
                Ip = IPAddress.Parse(data.IP),
                OrganizationId = orgId,
                Title = title,
                UserId = userId,
                TargetId = data.TargetId,
                Kind = AdminClearUserFrozenMessage.Type
            };
            _logDb.CoreLogs.Add(log);

            await _logDb.SaveChangesAsync(cancellationToken);
        }

        protected override async Task ProcessMessageAsync(AdminClearUserFrozenMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Culture
            var ci = LocalizationUtils.SetCulture(message.Data.Culture, true);
            Properties.Resources.Culture = ci;

            // User id
            var userId = (int)message.Data.TargetId;

            // Transaction for business logic related processing
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            // Clear the frozen time
            await _logDb.CoreLogs
                .Where(l => l.Kind == LoginFailedMessage.Type && l.UserId == userId)
                .OrderByDescending(l => l.Id)
                .Take(3)
                .ExecuteDeleteAsync(cancellationToken);

            await _db.CoreUsers.AsNoTracking()
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(u => u.SetProperty(u => u.FrozenTime, (DateTimeOffset?)null), cancellationToken);

            // Log the operation
            // For the operator
            await LogAsync(message, message.Data.UserId, message.Data.OrganizationId, cancellationToken);

            // For the requester
            await LogAsync(message, userId, null, cancellationToken);

            // Commit
            await transaction.CommitAsync(cancellationToken);
        }
    }
}
