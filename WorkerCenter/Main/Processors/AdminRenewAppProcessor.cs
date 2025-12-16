using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.Localization;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.LogDatabase.Models;
using PlatformShared.Messages;
using System.Net;
using WorkerCenter.Templates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Admin renew app processor
    /// 管理员续费应用处理器
    /// </summary>
    public class AdminRenewAppProcessor : CommonQueueProcessor<AdminRenewAppMessage>
    {
        private readonly LogDbContext _logDb;
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public AdminRenewAppProcessor(ILogger<AdminRenewAppProcessor> logger,
            LogDbContext logDb,
            MyDbContext db,
            IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.AdminRenewAppMessage)
        {
            _producer = producer;
            _logDb = logDb;
            _db = db;
        }

        private async Task LogAsync(AdminRenewAppMessage message, int userId, int? orgId, CancellationToken cancellationToken)
        {
            var data = message.Data;
            var title = $"{Properties.Resources.AdminRenewApp} ({data.TargetName})";

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
                Kind = AdminRenewAppMessage.Type,
                AppId = data.AppId
            };
            _logDb.CoreLogs.Add(log);

            await _logDb.SaveChangesAsync(cancellationToken);
        }

        protected override async Task ProcessMessageAsync(AdminRenewAppMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Culture
            var ci = LocalizationUtils.SetCulture(message.Data.Culture, true);
            Properties.Resources.Culture = ci;

            // Transaction for business logic related processing
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            // Update the expiry
            await _db.CoreOrganizationApps.AsNoTracking()
                .Where(oa => oa.Id == message.Data.TargetId)
                .ExecuteUpdateAsync(oa => oa.SetProperty(a => a.Expiry, a => a.Expiry == null ? DateTimeOffset.UtcNow.AddMonths(message.Months) : a.Expiry.Value.AddMonths(message.Months)), cancellationToken);

            // Log the operation
            // For the operator
            await LogAsync(message, message.Data.UserId, message.Data.OrganizationId, cancellationToken);

            // For the requester
            await LogAsync(message, message.Requester, message.RequesterOrgId, cancellationToken);

            // Commit
            await transaction.CommitAsync(cancellationToken);

            // Email notice
            // Emails
            var emails = await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [message.Requester], [message.Data.UserId], [message.Approver]);

            var subject = Properties.Resources.ActionNoticeSubject;
            var action = Properties.Resources.AdminRenewApp;
            var detail = Properties.Resources.AdminRenewAppDetail;

            // Load email template
            var data = new ActionNoticeData(message.Data,
                string.Format(subject, $"{action} - {message.Data.TargetName}"),
                action,
                string.Format(detail, message.Data.TargetName, message.Months, message.Comment, message.RequesterOrgName)
            );

            var body = await TemplateUtils.BuildNoticeTemplateAsync(message.Data.Culture, data, cancellationToken);

            // Send email notice
            var inviteeeEmail = new SendEmailMessage
            {
                Subject = data.Subject,
                Body = body,
                To = emails[0],
                Cc = emails[1],
                Bcc = emails[2]
            };

            await _producer.SendJsonAsync(inviteeeEmail, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
        }
    }
}
