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
    /// Buy app processor
    /// 购买应用处理器
    /// </summary>
    public class BuyAppProcessor : LogQueueProcessor<BuyAppMessage>
    {
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public BuyAppProcessor(ILogger<BuyAppProcessor> logger, LogDbContext logDb,
            MyDbContext db, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.BuyAppMessage, logDb)
        {
            _producer = producer;
            _db = db;
        }

        protected override async Task ProcessMessageAsync(BuyAppMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;

            // Organization id
            var orgId = message.OrgId;

            // New organization
            if (message.NewOrg)
            {
                // Organization name
                var orgName = await _db.CoreOrganizations
                    .AsNoTracking()
                    .Where(o => o.Id == orgId)
                    .Select(o => o.Name)
                    .FirstOrDefaultAsync(cancellationToken);

                // Create organization message
                var createOrgMessage = new CreateOrgMessage
                {
                    Data = message.Data with { TargetId = orgId, TargetName = orgName }
                };

                await _producer.SendJsonAsync(createOrgMessage, PlatformSharedContext.Default.CreateOrgMessage, CreateOrgMessage.Type, cancellationToken);
            }

            // All admins
            var admins = await _db.QueryUsersAsync(orgId, UserRole.Admin, cancellationToken);

            // Send email notice
            // Emails
            var allEmails = (await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId], admins));
            var emails = allEmails[0];
            var adminEmails = allEmails[1];

            if (emails.Length > 0 || adminEmails.Length > 0)
            {
                // Load email template
                var subject = Properties.Resources.ActionNoticeSubject;
                var action = Properties.Resources.BuyApp;

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
