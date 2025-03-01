using com.etsoo.Localization;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
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
    /// Admin tech support processor
    /// 管理员技术支持处理器
    /// </summary>
    public class AdminSupportProcessor : CommonQueueProcessor<AdminSupportMessage>
    {
        private readonly LogDbContext _logDb;
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public AdminSupportProcessor(ILogger<AdminSupportProcessor> logger,
            LogDbContext logDb,
            MyDbContext db,
            IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.AdminSupportMessage)
        {
            _producer = producer;
            _logDb = logDb;
            _db = db;
        }

        private async Task LogAsync(AdminSupportMessage message, int userId, int? orgId, CancellationToken cancellationToken)
        {
            var data = message.Data;
            var title = $"{Properties.Resources.AdminSupport} ({data.TargetName})";

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
                Kind = AdminSupportMessage.Type,
                AppId = data.AppId
            };
            _logDb.CoreLogs.Add(log);

            await _logDb.SaveChangesAsync(cancellationToken);
        }

        protected override async Task ProcessMessageAsync(AdminSupportMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Culture
            var ci = LocalizationUtils.SetCulture(message.Data.Culture, true);
            Properties.Resources.Culture = ci;

            // Transaction for business logic related processing
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            // Log the operation
            // For the operator
            await LogAsync(message, message.Data.UserId, message.Data.OrganizationId, cancellationToken);

            // For the requester
            await LogAsync(message, message.Requester, (int)message.Data.TargetId, cancellationToken);

            // Commit
            await transaction.CommitAsync(cancellationToken);

            // Email notice
            // Emails
            var emails = await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [message.Requester, message.OwnerId], [message.Data.UserId], [message.Approver]);

            var subject = Properties.Resources.ActionNoticeSubject;
            var action = Properties.Resources.AdminSupport;
            var detail = Properties.Resources.AdminSupportDetail;

            // Load email template
            var data = new ActionNoticeData(message.Data,
                string.Format(subject, $"{action}"),
                action,
                string.Format(detail, message.RequesterName, message.Data.UserName, message.ApproverName, message.Comment)
            );

            var body = await TemplateUtils.BuildTemplateAsync(TemplateUtils.ActionNoticeTemplate, data);

            // Send email notice
            var inviteeeEmail = new SendEmailMessage
            {
                Subject = data.Subject,
                Body = body,
                To = emails[0],
                Cc = emails[1],
                Bcc = emails[2]
            };

            await _producer.SendJsonAsync(inviteeeEmail, PlatformSharedContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
        }
    }
}
