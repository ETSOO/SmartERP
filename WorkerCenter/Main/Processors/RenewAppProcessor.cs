using com.etsoo.CoreFramework.Authentication;
using com.etsoo.MessageQueue;
using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using WorkerCenter.Templates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Renew app processor
    /// 应用续费处理器
    /// </summary>
    public class RenewAppProcessor : LogQueueProcessor<RenewAppMessage>
    {
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public RenewAppProcessor(ILogger<RenewAppProcessor> logger, LogDbContext logDb,
            MyDbContext db, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.RenewAppMessage, logDb)
        {
            _producer = producer;
            _db = db;
        }

        protected override async Task ProcessMessageAsync(RenewAppMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;

            // Organization id
            var orgId = message.Data.OrganizationId;

            // All admins
            var admins = await _db.CoreOrganizationUsers
                .Where(ou => ou.CoreOrganizationId == orgId && ou.UserRole >= UserRole.Admin)
                .Select(ou => ou.CoreUserId)
                .ToArrayAsync(cancellationToken);
            ;

            // Send email notice
            // Emails
            var allEmails = (await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId], admins));
            var emails = allEmails[0];
            var adminEmails = allEmails[1];

            if (emails.Length > 0 || adminEmails.Length > 0)
            {
                // Load email template
                var subject = Properties.Resources.ActionNoticeSubject;
                var action = Properties.Resources.RenewApp;

                var data = new ActionNoticeData(message.Data,
                    string.Format(subject, action),
                    $"{action} ({message.Data.TargetName})"
                );

                var body = await TemplateUtils.BuildTemplateAsync(TemplateUtils.ActionNoticeTemplate, data);

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = data.Subject,
                    Body = body,
                    To = emails,
                    Cc = adminEmails,
                    Importance = EmailImportance.High
                };

                await _producer.SendJsonAsync(email, PlatformSharedContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
