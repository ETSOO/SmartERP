using com.etsoo.CoreFramework.Authentication;
using com.etsoo.MessageQueue;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using System.Web;
using WorkerCenter.Templates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Delete member processor
    /// 删除成员处理器
    /// </summary>
    public class DeleteMemberProcessor : LogQueueProcessor<DeleteMemberMessage>
    {
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public DeleteMemberProcessor(ILogger<DeleteMemberProcessor> logger,
            LogDbContext logDb,
            MyDbContext db,
            IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.DeleteMemberMessage, logDb)
        {
            _producer = producer;
            _db = db;
        }

        protected override async Task ProcessMessageAsync(DeleteMemberMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Log
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User name
            var userName = HttpUtility.HtmlEncode(message.Data.UserName);

            // Invitee name
            var inviteeName = HttpUtility.HtmlEncode(message.Data.TargetName);

            // Inviter name
            var inviterName = HttpUtility.HtmlEncode(message.InviterName);

            // Organization owner
            var organizationId = message.Data.OrganizationId.GetValueOrDefault();

            // Owners
            var owners = await _db.QueryUsersAsync(organizationId, UserRole.Founder, cancellationToken);

            // Organization name
            var orgName = HttpUtility.HtmlEncode(message.OrgName);

            var subject = Properties.Resources.ActionNoticeSubject;

            // Emails
            var allEmails = await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [message.Data.UserId], [(int)message.Data.TargetId], owners);
            var emails = allEmails[0];
            var inviteeEmails = allEmails[1];
            var ownerEmails = allEmails[2];

            if (emails.Length > 0 || inviteeEmails.Length > 0 || ownerEmails.Length > 0)
            {
                var action = Properties.Resources.DeleteMember;
                action = $"{action} {inviteeName}";
                var detail = Properties.Resources.DeleteMemberDetail;

                // Load email template
                var data = new ActionNoticeData(message.Data,
                    string.Format(subject, action),
                    action,
                    string.Format(detail, userName, orgName, inviteeName, inviterName)
                );

                var body = await TemplateUtils.BuildTemplateAsync(TemplateUtils.ActionNoticeTemplate, data);

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = data.Subject,
                    Body = body,
                    To = emails,
                    Cc = inviteeEmails,
                    Bcc = ownerEmails
                };

                await _producer.SendJsonAsync(email, PlatformSharedContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
